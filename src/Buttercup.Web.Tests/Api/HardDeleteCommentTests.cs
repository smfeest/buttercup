using Buttercup.Web.TestUtils;
using HotChocolate;
using Xunit;

namespace Buttercup.Web.Api;

public sealed class HardDeleteCommentTests(AppFactory appFactory) : EndToEndTests(appFactory)
{
    [Fact]
    public async Task DeletingComment()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = true };
        var comment = this.ModelFactory.BuildComment(setRecipe: true);
        await this.DatabaseFixture.InsertEntities(currentUser, comment);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await PostHardDeleteCommentMutation(client, comment.Id);
        using var document = await response.Content.ReadAsJsonDocument();

        var deleted = ApiAssert
            .SuccessResponse(document)
            .GetProperty("hardDeleteComment")
            .GetProperty("deleted")
            .GetBoolean();
        Assert.True(deleted);
    }

    [Fact]
    public async Task DeletingCommentWhenNotAnAdmin()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = false };
        var comment = this.ModelFactory.BuildComment(setRecipe: true);
        await this.DatabaseFixture.InsertEntities(currentUser, comment);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await PostHardDeleteCommentMutation(client, comment.Id);
        using var document = await response.Content.ReadAsJsonDocument();

        JsonAssert.ValueIsNull(document.RootElement.GetProperty("data"));
        ApiAssert.HasSingleError(ErrorCodes.Authentication.NotAuthorized, document);
    }

    [Fact]
    public async Task DeletingNonExistentComment()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = true };
        await this.DatabaseFixture.InsertEntities(currentUser);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await PostHardDeleteCommentMutation(
            client, this.ModelFactory.NextInt());
        using var document = await response.Content.ReadAsJsonDocument();

        var deleted = ApiAssert
            .SuccessResponse(document)
            .GetProperty("hardDeleteComment")
            .GetProperty("deleted")
            .GetBoolean();
        Assert.False(deleted);
    }

    private static Task<HttpResponseMessage> PostHardDeleteCommentMutation(
        HttpClient client, long id) =>
        client.PostQuery("""
            mutation($id: Long!) {
                hardDeleteComment(input: { id: $id }) {
                    deleted
                }
            }
            """,
            new { id });
}
