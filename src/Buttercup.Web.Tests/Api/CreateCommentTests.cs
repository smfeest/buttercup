using Buttercup.Web.TestUtils;
using HotChocolate;
using Xunit;

namespace Buttercup.Web.Api;

public sealed class CreateCommentTests(AppFactory appFactory) : EndToEndTests(appFactory)
{
    [Fact]
    public async Task CreatingComment()
    {
        var currentUser = this.ModelFactory.BuildUser();
        var recipe = this.ModelFactory.BuildRecipe();
        await this.DatabaseFixture.InsertEntities(currentUser, recipe);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);

        var body = this.ModelFactory.NextString("body");

        using var response = await PostCreateCommentMutation(client, recipe.Id, body);
        using var document = await response.Content.ReadAsJsonDocument();

        var createCommentElement = ApiAssert.SuccessResponse(document).GetProperty("createComment");
        var recipeElement = createCommentElement.GetProperty("comment");
        var id = recipeElement.GetProperty("id").GetInt64();

        var expected = new
        {
            id,
            Recipe = new { recipe.Id, recipe.Title },
            Author = IdName.From(currentUser),
            body,
            Revision = 0
        };
        JsonAssert.Equivalent(expected, recipeElement);

        JsonAssert.ValueIsNull(createCommentElement.GetProperty("errors"));
    }

    [Fact]
    public async Task CreatingCommentWhenUnauthenticated()
    {
        using var client = this.AppFactory.CreateClient();
        using var response = await PostCreateCommentMutation(
            client, this.ModelFactory.NextInt(), this.ModelFactory.NextString("body"));
        using var document = await response.Content.ReadAsJsonDocument();

        JsonAssert.ValueIsNull(document.RootElement.GetProperty("data"));
        ApiAssert.HasSingleError(ErrorCodes.Authentication.NotAuthenticated, document);
    }

    [Fact]
    public async Task CreatingCommentForNonExistentRecipe()
    {
        var currentUser = this.ModelFactory.BuildUser();
        await this.DatabaseFixture.InsertEntities(currentUser);

        var recipeId = this.ModelFactory.NextInt();

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await PostCreateCommentMutation(
            client, recipeId, this.ModelFactory.NextString("body"));
        using var document = await response.Content.ReadAsJsonDocument();

        var createCommentElement = ApiAssert.SuccessResponse(document).GetProperty("createComment");
        JsonAssert.ValueIsNull(createCommentElement.GetProperty("comment"));
        var expectedErrors = new[]
        {
            new
            {
                __typename = "NotFoundError",
                Message = $"Recipe/{recipeId} not found",
            },
        };
        JsonAssert.Equivalent(expectedErrors, createCommentElement.GetProperty("errors"));
    }

    [Fact]
    public async Task CreatingCommentForSoftDeletedRecipe()
    {
        var currentUser = this.ModelFactory.BuildUser();
        var recipe = this.ModelFactory.BuildRecipe(softDeleted: true);
        await this.DatabaseFixture.InsertEntities(currentUser, recipe);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await PostCreateCommentMutation(
            client, recipe.Id, this.ModelFactory.NextString("body"));
        using var document = await response.Content.ReadAsJsonDocument();

        var createCommentElement = ApiAssert.SuccessResponse(document).GetProperty("createComment");
        JsonAssert.ValueIsNull(createCommentElement.GetProperty("comment"));
        var expectedErrors = new[]
        {
            new
            {
                __typename = "SoftDeletedError",
                Message = $"Cannot add comment to soft-deleted recipe {recipe.Id}",
            },
        };
        JsonAssert.Equivalent(expectedErrors, createCommentElement.GetProperty("errors"));
    }

    [Fact]
    public async Task CreatingCommentWithInvalidBody()
    {
        var currentUser = this.ModelFactory.BuildUser();
        await this.DatabaseFixture.InsertEntities(currentUser);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);

        using var response = await PostCreateCommentMutation(
            client, this.ModelFactory.NextInt(), string.Empty);
        using var document = await response.Content.ReadAsJsonDocument();

        var createCommentElement = ApiAssert.SuccessResponse(document).GetProperty("createComment");

        JsonAssert.ValueIsNull(createCommentElement.GetProperty("comment"));

        var expectedErrors = new[]
        {
            new
            {
                Message = "Comment cannot be empty",
                Path = new string[] { "input", "attributes", "body" },
                Code = "REQUIRED",
            },
        };
        JsonAssert.Equivalent(expectedErrors, createCommentElement.GetProperty("errors"));
    }

    private static Task<HttpResponseMessage> PostCreateCommentMutation(
        HttpClient client, long recipeId, string body) =>
        client.PostQuery("""
            mutation($recipeId: Long!, $body: String!) {
                createComment(input: { recipeId: $recipeId, attributes: { body: $body } }) {
                    comment {
                        id
                        recipe { id title }
                        author { id name }
                        body
                        revision
                    }
                    errors {
                        __typename
                        ... on Error {
                            message
                        }
                        ... on ValidationError {
                            path
                            code
                        }
                    }
                }
            }
            """,
            new { recipeId, body });
}
