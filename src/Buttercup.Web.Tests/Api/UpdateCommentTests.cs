using Buttercup.Application;
using Buttercup.Web.TestUtils;
using HotChocolate;
using Xunit;

namespace Buttercup.Web.Api;

public sealed class UpdateCommentTests(AppFactory appFactory) : EndToEndTests(appFactory)
{
    [Fact]
    public async Task UpdatingOwnComment()
    {
        var recipe = this.ModelFactory.BuildRecipe();
        var author = this.ModelFactory.BuildUser() with { IsAdmin = false };
        var comment = this.ModelFactory.BuildComment(recipe) with { Author = author };
        await this.DatabaseFixture.InsertEntities(comment);

        using var client = await this.AppFactory.CreateClientForApiUser(author);

        var attributes = this.BuildCommentAttributes();

        using var response = await PostUpdateCommentMutation(
            client, comment.Id, comment.UpdateCount, attributes);
        using var document = await response.Content.ReadAsJsonDocument();

        var updateCommentElement = ApiAssert.SuccessResponse(document).GetProperty("updateComment");
        var commentElement = updateCommentElement.GetProperty("comment");

        var expected = new
        {
            comment.Id,
            Author = IdName.From(author),
            attributes.Body,
            UpdateCount = comment.UpdateCount + 1,
        };
        JsonAssert.Equivalent(expected, commentElement);

        JsonAssert.ValueIsNull(updateCommentElement.GetProperty("errors"));
    }

    [Fact]
    public async Task UpdatingCommentWhenUnauthenticated()
    {
        var comment = this.ModelFactory.BuildComment(this.ModelFactory.BuildRecipe());

        using var client = this.AppFactory.CreateClient();
        using var response = await PostUpdateCommentMutation(
            client, comment.Id, comment.UpdateCount, this.BuildCommentAttributes());
        using var document = await response.Content.ReadAsJsonDocument();

        JsonAssert.ValueIsNull(document.RootElement.GetProperty("data"));
        ApiAssert.HasSingleError(ErrorCodes.Authentication.NotAuthenticated, document);
    }

    [Fact]
    public async Task UpdatingNonExistentComment()
    {
        var currentUser = this.ModelFactory.BuildUser();
        await this.DatabaseFixture.InsertEntities(currentUser);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);

        var commentId = this.ModelFactory.NextInt();

        using var response = await PostUpdateCommentMutation(
            client, commentId, 0, this.BuildCommentAttributes());
        using var document = await response.Content.ReadAsJsonDocument();

        var updateCommentElement = ApiAssert.SuccessResponse(document).GetProperty("updateComment");

        JsonAssert.ValueIsNull(updateCommentElement.GetProperty("comment"));

        var expectedErrors = new[]
        {
            new
            {
                __typename = "NotFoundError",
                Message = $"Comment/{commentId} not found",
            },
        };
        JsonAssert.Equivalent(expectedErrors, updateCommentElement.GetProperty("errors"));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task UpdateAnotherUsersComment(bool isAdmin)
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = isAdmin };
        var recipe = this.ModelFactory.BuildRecipe();
        var author = this.ModelFactory.BuildUser();
        var comment = this.ModelFactory.BuildComment(recipe) with { Author = author };
        await this.DatabaseFixture.InsertEntities(currentUser, comment);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);

        using var response = await PostUpdateCommentMutation(
            client, comment.Id, comment.UpdateCount, this.BuildCommentAttributes());
        using var document = await response.Content.ReadAsJsonDocument();

        JsonAssert.ValueIsNull(document.RootElement.GetProperty("data"));

        ApiAssert.HasSingleError(
            ErrorCodes.Authentication.NotAuthorized,
            "The current user is not authorized to update this comment",
            document);
    }

    [Fact]
    public async Task UpdatingCommentOnSoftDeletedRecipe()
    {
        var recipe = this.ModelFactory.BuildRecipe(softDeleted: true);
        var author = this.ModelFactory.BuildUser();
        var comment = this.ModelFactory.BuildComment(recipe) with { Author = author };
        await this.DatabaseFixture.InsertEntities(comment);

        using var client = await this.AppFactory.CreateClientForApiUser(author);

        using var response = await PostUpdateCommentMutation(
            client, comment.Id, comment.UpdateCount, this.BuildCommentAttributes());
        using var document = await response.Content.ReadAsJsonDocument();

        var updateCommentElement = ApiAssert.SuccessResponse(document).GetProperty("updateComment");

        JsonAssert.ValueIsNull(updateCommentElement.GetProperty("comment"));

        var expectedErrors = new[]
        {
            new
            {
                __typename = "SoftDeletedError",
                Message = $"Cannot update comment {comment.Id} on soft-deleted recipe {recipe.Id}",
            },
        };
        JsonAssert.Equivalent(expectedErrors, updateCommentElement.GetProperty("errors"));
    }

    [Fact]
    public async Task UpdatingSoftDeletedComment()
    {
        var recipe = this.ModelFactory.BuildRecipe();
        var author = this.ModelFactory.BuildUser();
        var comment = this.ModelFactory.BuildComment(recipe, softDeleted: true) with
        {
            Author = author,
        };
        await this.DatabaseFixture.InsertEntities(comment);

        using var client = await this.AppFactory.CreateClientForApiUser(author);

        using var response = await PostUpdateCommentMutation(
            client, comment.Id, comment.UpdateCount, this.BuildCommentAttributes());
        using var document = await response.Content.ReadAsJsonDocument();

        var updateCommentElement = ApiAssert.SuccessResponse(document).GetProperty("updateComment");

        JsonAssert.ValueIsNull(updateCommentElement.GetProperty("comment"));

        var expectedErrors = new[]
        {
            new
            {
                __typename = "SoftDeletedError",
                Message = $"Cannot update soft-deleted comment {comment.Id}",
            },
        };
        JsonAssert.Equivalent(expectedErrors, updateCommentElement.GetProperty("errors"));
    }

    [Fact]
    public async Task UpdatingCommentWithStaleBaseUpdateCount()
    {
        var recipe = this.ModelFactory.BuildRecipe();
        var author = this.ModelFactory.BuildUser();
        var comment = this.ModelFactory.BuildComment(recipe) with { Author = author };
        await this.DatabaseFixture.InsertEntities(comment);

        using var client = await this.AppFactory.CreateClientForApiUser(author);

        using var response = await PostUpdateCommentMutation(
            client, comment.Id, comment.UpdateCount - 1, this.BuildCommentAttributes());
        using var document = await response.Content.ReadAsJsonDocument();

        var updateCommentElement = ApiAssert.SuccessResponse(document).GetProperty("updateComment");

        JsonAssert.ValueIsNull(updateCommentElement.GetProperty("comment"));

        var expectedErrors = new[]
        {
            new
            {
                __typename = "ConcurrencyError",
                Message = $"Base update count {comment.UpdateCount - 1} does not match current update count {comment.UpdateCount}",
            },
        };
        JsonAssert.Equivalent(expectedErrors, updateCommentElement.GetProperty("errors"));
    }

    [Fact]
    public async Task UpdatingCommentWithInvalidAttributes()
    {
        var recipe = this.ModelFactory.BuildRecipe();
        var author = this.ModelFactory.BuildUser();
        var comment = this.ModelFactory.BuildComment(recipe) with { Author = author };
        await this.DatabaseFixture.InsertEntities(comment);

        using var client = await this.AppFactory.CreateClientForApiUser(author);

        using var response = await PostUpdateCommentMutation(
            client, comment.Id, comment.UpdateCount, new() { Body = string.Empty });
        using var document = await response.Content.ReadAsJsonDocument();

        var updateCommentElement = ApiAssert.SuccessResponse(document).GetProperty("updateComment");

        JsonAssert.ValueIsNull(updateCommentElement.GetProperty("comment"));

        var expectedErrors = new[]
        {
            new
            {
                __typename = "ValidationError",
                Message = "Comment cannot be empty",
                Path = new string[] { "input", "attributes", "body" },
                Code = "REQUIRED",
            },
        };
        JsonAssert.Equivalent(expectedErrors, updateCommentElement.GetProperty("errors"));
    }

    private CommentAttributes BuildCommentAttributes() =>
        new() { Body = this.ModelFactory.NextString("comment-body") };

    private static Task<HttpResponseMessage> PostUpdateCommentMutation(
        HttpClient client, long id, int baseUpdateCount, CommentAttributes attributes) =>
        client.PostQuery("""
            mutation($input: UpdateCommentInput!) {
                updateComment(input: $input) {
                    comment {
                        id
                        author { id name }
                        body
                        updateCount
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
            new { input = new { id, baseUpdateCount, attributes } });
}
