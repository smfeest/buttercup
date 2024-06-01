using Buttercup.EntityModel;
using Buttercup.Web.TestUtils;
using Xunit;

namespace Buttercup.Web.Api;

public sealed class RecipeTests(AppFactory<RecipeTests> appFactory)
    : EndToEndTests<RecipeTests>(appFactory)
{
    [Theory]
    [InlineData(true, false)]
    [InlineData(false, false)]
    [InlineData(true, true)]
    public async Task QueryingRecipe(bool setOptionalAttributes, bool softDeleted)
    {
        var currentUser = this.ModelFactory.BuildUser();
        var recipe = this.ModelFactory.BuildRecipe(setOptionalAttributes, softDeleted);
        recipe.Revisions.Add(
            RecipeRevision.From(this.ModelFactory.BuildRecipe(setOptionalAttributes)));
        var comment = this.ModelFactory.BuildComment(setOptionalAttributes: true);
        comment.Revisions.Add(
            CommentRevision.From(this.ModelFactory.BuildComment(setOptionalAttributes: true)));
        recipe.Comments.Add(comment);
        await this.DatabaseFixture.InsertEntities(currentUser, recipe);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await PostRecipeQuery(client, recipe.Id);
        using var document = await response.Content.ReadAsJsonDocument();

        var dataElement = ApiAssert.SuccessResponse(document);

        var expected = new
        {
            recipe.Id,
            recipe.Title,
            recipe.PreparationMinutes,
            recipe.CookingMinutes,
            recipe.Servings,
            recipe.Ingredients,
            recipe.Method,
            recipe.Suggestions,
            recipe.Remarks,
            recipe.Source,
            recipe.Created,
            CreatedByUser = recipe.CreatedByUser == null ?
                null :
                new { recipe.CreatedByUser.Id, recipe.CreatedByUser.Name },
            recipe.Modified,
            ModifiedByUser = recipe.ModifiedByUser == null ?
                null :
                new { recipe.ModifiedByUser.Id, recipe.ModifiedByUser.Name },
            recipe.Deleted,
            DeletedByUser = recipe.DeletedByUser == null ?
                null :
                new { recipe.DeletedByUser.Id, recipe.DeletedByUser.Name },
            recipe.Revision,
            revisions = recipe.Revisions.Select(revision => new
            {
                revision.Revision,
                revision.Created,
                CreatedByUser = revision.CreatedByUser == null ?
                    null :
                    new { revision.CreatedByUser.Id, revision.CreatedByUser.Name },
                revision.Title,
                revision.PreparationMinutes,
                revision.CookingMinutes,
                revision.Servings,
                revision.Ingredients,
                revision.Method,
                revision.Suggestions,
                revision.Remarks,
                revision.Source,
            }),
            Comments = recipe.Comments.Select(comment => new
            {
                comment.Id,
                Author = new { comment.Author?.Id, comment.Author?.Name },
                comment.Body,
                comment.Created,
                comment.Modified,
                comment.Revision,
                Revisions = comment.Revisions.Select(revision => new
                {
                    revision.Revision,
                    revision.Created,
                    revision.Body
                }),
            }),
        };

        JsonAssert.Equivalent(expected, dataElement.GetProperty("recipe"));
    }

    [Fact]
    public async Task QueryingNonExistentRecipe()
    {
        var currentUser = this.ModelFactory.BuildUser();
        var recipe = this.ModelFactory.BuildRecipe();
        await this.DatabaseFixture.InsertEntities(currentUser, recipe);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await PostRecipeQuery(client, this.ModelFactory.NextInt());
        using var document = await response.Content.ReadAsJsonDocument();

        var dataElement = ApiAssert.SuccessResponse(document);

        JsonAssert.ValueIsNull(dataElement.GetProperty("recipe"));
    }

    [Fact]
    public async Task QueryingRecipeWhenUnauthenticated()
    {
        var recipe = this.ModelFactory.BuildRecipe();
        await this.DatabaseFixture.InsertEntities(recipe);

        using var client = this.AppFactory.CreateClient();
        using var response = await PostRecipeQuery(client, recipe.Id);
        using var document = await response.Content.ReadAsJsonDocument();

        JsonAssert.ValueIsNull(document.RootElement.GetProperty("data").GetProperty("recipe"));

        ApiAssert.HasSingleError("AUTH_NOT_AUTHORIZED", document);
    }

    private static Task<HttpResponseMessage> PostRecipeQuery(HttpClient client, long id) =>
        client.PostQuery(
            @"query($id: Long!) {
                recipe(id: $id) {
                    id
                    title
                    preparationMinutes
                    cookingMinutes
                    servings
                    ingredients
                    method
                    suggestions
                    remarks
                    source
                    created
                    createdByUser { id name }
                    modified
                    modifiedByUser { id name }
                    deleted
                    deletedByUser { id name }
                    revision
                    revisions {
                        revision
                        created
                        createdByUser { id name }
                        title
                        preparationMinutes
                        cookingMinutes
                        servings
                        ingredients
                        method
                        suggestions
                        remarks
                        source
                    }
                    comments {
                        id
                        author { id name }
                        body
                        created
                        modified
                        revision
                        revisions { revision created body }
                    }
                }
            }",
            new { id });
}
