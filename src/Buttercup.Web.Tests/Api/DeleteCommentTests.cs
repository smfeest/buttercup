using Buttercup.Web.TestUtils;
using Xunit;

namespace Buttercup.Web.Api;

public sealed class DeleteCommentTests(AppFactory<DeleteCommentTests> appFactory)
    : EndToEndTests<DeleteCommentTests>(appFactory)
{
    [Fact]
    public async Task DeletingOwnCommentWhenNotAnAdmin()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = false };
        var comment = this.ModelFactory.BuildComment(setRecipe: true) with { Author = currentUser };
        await this.DatabaseFixture.InsertEntities(currentUser, comment);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await PostDeleteCommentMutation(client, comment.Id);
        using var document = await response.Content.ReadAsJsonDocument();

        var expected = new
        {
            Deleted = true,
            Comment = new
            {
                comment.Id,
                comment.Body,
                DeletedByUser = new { currentUser.Id, currentUser.Name },
            },
        };
        var actual = ApiAssert.SuccessResponse(document).GetProperty("deleteComment");

        JsonAssert.Equivalent(expected, actual);
    }

    [Fact]
    public async Task DeletingSomeoneElsesCommentWhenNotAnAdmin()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = false };
        var comment = this.ModelFactory.BuildComment(setOptionalAttributes: true, setRecipe: true);
        await this.DatabaseFixture.InsertEntities(currentUser, comment);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await PostDeleteCommentMutation(client, comment.Id);
        using var document = await response.Content.ReadAsJsonDocument();

        JsonAssert.ValueIsNull(document.RootElement.GetProperty("data"));
        ApiAssert.HasSingleError(
            "AUTH_NOT_AUTHORIZED",
            "The current user is not authorized to delete this comment",
            document);
    }

    [Fact]
    public async Task DeletingSomeoneElsesCommentWhenAnAdmin()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = true };
        var comment = this.ModelFactory.BuildComment(setOptionalAttributes: true, setRecipe: true);
        await this.DatabaseFixture.InsertEntities(currentUser, comment);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await PostDeleteCommentMutation(client, comment.Id);
        using var document = await response.Content.ReadAsJsonDocument();

        var expected = new
        {
            Deleted = true,
            Comment = new
            {
                comment.Id,
                comment.Body,
                DeletedByUser = new { currentUser.Id, currentUser.Name },
            },
        };
        var actual = ApiAssert.SuccessResponse(document).GetProperty("deleteComment");

        JsonAssert.Equivalent(expected, actual);
    }

    [Fact]
    public async Task DeletingCommentWhenUnauthenticated()
    {
        var comment = this.ModelFactory.BuildComment(setOptionalAttributes: true, setRecipe: true);
        await this.DatabaseFixture.InsertEntities(comment);

        using var client = this.AppFactory.CreateClient();
        using var response = await PostDeleteCommentMutation(client, comment.Id);
        using var document = await response.Content.ReadAsJsonDocument();

        JsonAssert.ValueIsNull(document.RootElement.GetProperty("data"));
        ApiAssert.HasSingleError("AUTH_NOT_AUTHORIZED", document);
    }

    [Fact]
    public async Task DeletingOwnAlreadySoftDeletedComment()
    {
        var currentUser = this.ModelFactory.BuildUser();
        var comment = this.ModelFactory.BuildComment(
            setRecipe: true, setOptionalAttributes: true, softDeleted: true) with
        {
            Author = currentUser
        };
        await this.DatabaseFixture.InsertEntities(currentUser, comment);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await PostDeleteCommentMutation(client, comment.Id);
        using var document = await response.Content.ReadAsJsonDocument();

        var expected = new
        {
            Deleted = false,
            Comment = new
            {
                comment.Id,
                comment.Body,
                DeletedByUser = new { comment.DeletedByUser?.Id, comment.DeletedByUser?.Name },
            },
        };
        var actual = ApiAssert.SuccessResponse(document).GetProperty("deleteComment");
        JsonAssert.Equivalent(expected, actual);
    }

    [Fact]
    public async Task DeletingNonExistentCommentWhenNotAnAdmin()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = false };
        await this.DatabaseFixture.InsertEntities(currentUser);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await PostDeleteCommentMutation(client, this.ModelFactory.NextInt());
        using var document = await response.Content.ReadAsJsonDocument();

        JsonAssert.ValueIsNull(document.RootElement.GetProperty("data"));
        ApiAssert.HasSingleError("AUTH_NOT_AUTHORIZED", document);
    }

    [Fact]
    public async Task DeletingNonExistentCommentWhenAnAdmin()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = true };
        await this.DatabaseFixture.InsertEntities(currentUser);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await PostDeleteCommentMutation(client, this.ModelFactory.NextInt());
        using var document = await response.Content.ReadAsJsonDocument();

        var deleteCommentElement = ApiAssert.SuccessResponse(document).GetProperty("deleteComment");

        Assert.False(deleteCommentElement.GetProperty("deleted").GetBoolean());
        JsonAssert.ValueIsNull(deleteCommentElement.GetProperty("comment"));
    }

    private static Task<HttpResponseMessage> PostDeleteCommentMutation(
        HttpClient client, long id) =>
        client.PostQuery(
            @"mutation($id: Long!) {
                deleteComment(input: { id: $id }) {
                    deleted
                    comment {
                        id
                        body
                        deletedByUser { id name }
                    }
                }
            }",
            new { id });
}
