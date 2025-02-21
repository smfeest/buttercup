using Buttercup.EntityModel;
using Buttercup.Web.TestUtils;
using HotChocolate;
using Xunit;

namespace Buttercup.Web.Api;

public sealed class RecipeTests(AppFactory appFactory) : EndToEndTests(appFactory)
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task QueryingRecipe(bool setOptionalAttributes)
    {
        var currentUser = this.ModelFactory.BuildUser();
        var recipe = this.ModelFactory.BuildRecipe(setOptionalAttributes);
        recipe.Revisions.Add(
            RecipeRevision.From(this.ModelFactory.BuildRecipe(setOptionalAttributes)));
        var comment = this.ModelFactory.BuildComment(setOptionalAttributes: true);
        comment.Revisions.Add(
            CommentRevision.From(this.ModelFactory.BuildComment(setOptionalAttributes: true)));
        recipe.Comments.Add(comment);
        recipe.Comments.Add(this.ModelFactory.BuildComment(softDeleted: true));
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
            CreatedByUser = IdName.From(recipe.CreatedByUser),
            recipe.Modified,
            ModifiedByUser = IdName.From(recipe.ModifiedByUser),
            recipe.Deleted,
            DeletedByUser = IdName.From(recipe.DeletedByUser),
            recipe.Revision,
            revisions = recipe.Revisions.Select(revision => new
            {
                revision.Revision,
                revision.Created,
                CreatedByUser = IdName.From(revision.CreatedByUser),
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
            Comments = new[]
            {
                new
                {
                    comment.Id,
                    Author = IdName.From(comment.Author),
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
                }
            },
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

        ApiAssert.HasSingleError(ErrorCodes.Authentication.NotAuthenticated, document);
    }

    [Fact]
    public async Task QueryingDeletedRecipeWhenAnAdmin()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = true };
        var recipe = this.ModelFactory.BuildRecipe(softDeleted: true);
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
            CreatedByUser = IdName.From(recipe.CreatedByUser),
            recipe.Modified,
            ModifiedByUser = IdName.From(recipe.ModifiedByUser),
            recipe.Deleted,
            DeletedByUser = IdName.From(recipe.DeletedByUser),
            recipe.Revision,
            revisions = Array.Empty<object>(),
            Comments = Array.Empty<object>()
        };

        JsonAssert.Equivalent(expected, dataElement.GetProperty("recipe"));
    }

    [Fact]
    public async Task QueryingDeletedRecipeWhenNotAnAdmin()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = false };
        var recipe = this.ModelFactory.BuildRecipe(softDeleted: true);
        await this.DatabaseFixture.InsertEntities(currentUser, recipe);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await PostRecipeQuery(client, recipe.Id);
        using var document = await response.Content.ReadAsJsonDocument();

        JsonAssert.ValueIsNull(document.RootElement.GetProperty("data").GetProperty("recipe"));

        ApiAssert.HasSingleError(ErrorCodes.Authentication.NotAuthorized, document);
    }

    [Fact]
    public async Task QueryingDeletedCommentsWhenAnAdmin()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = true };
        var recipe = this.ModelFactory.BuildRecipe();
        var deletedComment = this.ModelFactory.BuildComment(
            setOptionalAttributes: true, softDeleted: true);
        recipe.Comments.Add(deletedComment);
        recipe.Comments.Add(this.ModelFactory.BuildComment(softDeleted: false));
        await this.DatabaseFixture.InsertEntities(currentUser, recipe);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await PostDeletedCommentsQuery(client, recipe.Id);
        using var document = await response.Content.ReadAsJsonDocument();

        var dataElement = ApiAssert.SuccessResponse(document);

        var expected = new[]
        {
            new
            {
                deletedComment.Id,
                deletedComment.Deleted,
                DeletedByUser = IdName.From(deletedComment.DeletedByUser),
            }
        };

        JsonAssert.Equivalent(
            expected, dataElement.GetProperty("recipe").GetProperty("deletedComments"));
    }

    [Fact]
    public async Task QueryingDeletedCommentsWhenNotAnAdmin()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = false };
        var recipe = this.ModelFactory.BuildRecipe();
        recipe.Comments.Add(this.ModelFactory.BuildComment(softDeleted: true));
        await this.DatabaseFixture.InsertEntities(currentUser, recipe);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await PostDeletedCommentsQuery(client, recipe.Id);
        using var document = await response.Content.ReadAsJsonDocument();

        JsonAssert.ValueIsNull(document.RootElement.GetProperty("data").GetProperty("recipe"));

        var expectedErrors = new[]
        {
            new
            {
                Path = new string[] { "recipe", "deletedComments" },
                Extensions = new { Code = ErrorCodes.Authentication.NotAuthorized },
            },
        };

        Assert.True(document.RootElement.TryGetProperty("errors", out var errorsElement));
        JsonAssert.Equivalent(expectedErrors, errorsElement);
    }

    private static Task<HttpResponseMessage> PostRecipeQuery(HttpClient client, long id) =>
        client.PostQuery("""
            query($id: Long!) {
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
            }
            """,
            new { id });

    private static Task<HttpResponseMessage> PostDeletedCommentsQuery(HttpClient client, long id) =>
        client.PostQuery("""
            query($id: Long!) {
                recipe(id: $id) {
                    deletedComments {
                        id
                        deleted
                        deletedByUser { id name }
                    }
                }
            }
            """,
            new { id });
}
