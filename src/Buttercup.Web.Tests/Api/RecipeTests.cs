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
                new { recipe.CreatedByUser.Id, recipe.CreatedByUser.Email },
            recipe.Modified,
            ModifiedByUser = recipe.ModifiedByUser == null ?
                null :
                new { recipe.ModifiedByUser.Id, recipe.ModifiedByUser.Email },
            recipe.Deleted,
            DeletedByUser = recipe.DeletedByUser == null ?
                null :
                new { recipe.DeletedByUser.Id, recipe.DeletedByUser.Email },
            recipe.Revision,
            revisions = recipe.Revisions.Select(revision => new
            {
                revision.Revision,
                revision.Created,
                CreatedByUser = revision.CreatedByUser == null ?
                    null :
                    new { revision.CreatedByUser.Id, revision.CreatedByUser.Email },
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
                    createdByUser { id email }
                    modified
                    modifiedByUser { id email }
                    deleted
                    deletedByUser { id email }
                    revision
                    revisions {
                        revision
                        created
                        createdByUser { id email }
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
                }
            }",
            new { id });
}
