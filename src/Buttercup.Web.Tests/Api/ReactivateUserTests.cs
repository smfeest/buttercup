using Buttercup.Web.TestUtils;
using HotChocolate;
using Xunit;

namespace Buttercup.Web.Api;

public sealed class ReactivateUserTests(AppFactory appFactory) : EndToEndTests(appFactory)
{
    [Fact]
    public async Task ReactivatingUser()
    {
        var user = this.ModelFactory.BuildUser(deactivated: true);
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = true };
        await this.DatabaseFixture.InsertEntities(user, currentUser);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await PostReactivateUserMutation(client, user.Id);
        using var document = await response.Content.ReadAsJsonDocument();

        var actualPayload = ApiAssert.SuccessResponse(document).GetProperty("reactivateUser");
        var timestamp = actualPayload.GetProperty("user").GetProperty("modified").GetDateTime();
        var expectedPayload = new
        {
            Reactivated = true,
            User = new
            {
                user.Id,
                user.Name,
                Modified = timestamp,
                Deactivated = (DateTime?)null,
                Revision = user.Revision + 1,
            },
            Errors = (object?)null,
        };
        JsonAssert.Equivalent(expectedPayload, actualPayload);
    }

    [Fact]
    public async Task ReactivatingUserWhenNotAnAdmin()
    {
        var user = this.ModelFactory.BuildUser(deactivated: true);
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = false };
        await this.DatabaseFixture.InsertEntities(user, currentUser);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await PostReactivateUserMutation(client, user.Id);
        using var document = await response.Content.ReadAsJsonDocument();

        JsonAssert.ValueIsNull(document.RootElement.GetProperty("data"));
        ApiAssert.HasSingleError(ErrorCodes.Authentication.NotAuthorized, document);
    }

    [Fact]
    public async Task ReactivatingAlreadyActiveUser()
    {
        var user = this.ModelFactory.BuildUser();
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = true };
        await this.DatabaseFixture.InsertEntities(user, currentUser);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await PostReactivateUserMutation(client, user.Id);
        using var document = await response.Content.ReadAsJsonDocument();

        var actualPayload = ApiAssert.SuccessResponse(document).GetProperty("reactivateUser");
        var expectedPayload = new
        {
            Reactivated = false,
            User = new
            {
                user.Id,
                user.Name,
                user.Modified,
                user.Deactivated,
                user.Revision,
            },
            Errors = (object?)null,
        };
        JsonAssert.Equivalent(expectedPayload, actualPayload);
    }

    [Fact]
    public async Task ReactivatingNonExistentUser()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = true };
        await this.DatabaseFixture.InsertEntities(currentUser);

        var nonExistentUserId = this.ModelFactory.NextInt();

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await PostReactivateUserMutation(client, nonExistentUserId);
        using var document = await response.Content.ReadAsJsonDocument();

        var actualPayload = ApiAssert.SuccessResponse(document).GetProperty("reactivateUser");
        var expectedPayload = new
        {
            Reactivated = (bool?)null,
            User = (object?)null,
            Errors = new[]
            {
                new
                {
                    __typename = "NotFoundError",
                    Message = $"User/{nonExistentUserId} not found",
                },
            },
        };
        JsonAssert.Equivalent(expectedPayload, actualPayload);
    }

    private static Task<HttpResponseMessage> PostReactivateUserMutation(
        HttpClient client, long id) =>
        client.PostQuery("""
            mutation($id: Long!) {
                reactivateUser(input: { id: $id }) {
                    reactivated
                    user {
                        id
                        name
                        modified
                        deactivated
                        revision
                    }
                    errors {
                        __typename
                        ... on Error {
                            message
                        }
                    }
                }
            }
            """,
            new { id });
}
