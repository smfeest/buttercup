using Buttercup.Web.TestUtils;
using HotChocolate;
using Xunit;

namespace Buttercup.Web.Api;

public sealed class HardDeleteTestUserTests(AppFactory appFactory) : EndToEndTests(appFactory)
{
    [Fact]
    public async Task DeletingTestUser()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = true };
        var testUser = this.ModelFactory.BuildUser();
        await this.DatabaseFixture.InsertEntities(currentUser, testUser);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await PostHardDeleteTestUserMutation(client, testUser.Id);
        using var document = await response.Content.ReadAsJsonDocument();

        var deleted = ApiAssert
            .SuccessResponse(document)
            .GetProperty("hardDeleteTestUser")
            .GetProperty("deleted")
            .GetBoolean();
        Assert.True(deleted);
    }

    [Fact]
    public async Task DeletingTestUserWhenNotAnAdmin()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = false };
        var testUser = this.ModelFactory.BuildUser();
        await this.DatabaseFixture.InsertEntities(currentUser, testUser);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await PostHardDeleteTestUserMutation(client, testUser.Id);
        using var document = await response.Content.ReadAsJsonDocument();

        JsonAssert.ValueIsNull(document.RootElement.GetProperty("data"));
        ApiAssert.HasSingleError(ErrorCodes.Authentication.NotAuthorized, document);
    }

    [Fact]
    public async Task DeletingNonExistentTestUser()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = true };
        await this.DatabaseFixture.InsertEntities(currentUser);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await PostHardDeleteTestUserMutation(
            client, this.ModelFactory.NextInt());
        using var document = await response.Content.ReadAsJsonDocument();

        var deleted = ApiAssert
            .SuccessResponse(document)
            .GetProperty("hardDeleteTestUser")
            .GetProperty("deleted")
            .GetBoolean();
        Assert.False(deleted);
    }

    private static Task<HttpResponseMessage> PostHardDeleteTestUserMutation(
        HttpClient client, long id) =>
        client.PostQuery("""
            mutation($id: Long!) {
                hardDeleteTestUser(input: { id: $id }) {
                    deleted
                }
            }
            """,
            new { id });
}
