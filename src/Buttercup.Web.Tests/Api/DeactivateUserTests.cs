using Buttercup.Web.TestUtils;
using HotChocolate;
using Xunit;

namespace Buttercup.Web.Api;

public sealed class DeactivateUserTests(AppFactory appFactory) : EndToEndTests(appFactory)
{
    [Fact]
    public async Task DeactivatingUser()
    {
        var user = this.ModelFactory.BuildUser();
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = true };
        await this.DatabaseFixture.InsertEntities(user, currentUser);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await PostDeactivateUserMutation(client, user.Id);
        using var document = await response.Content.ReadAsJsonDocument();

        var actualPayload = ApiAssert.SuccessResponse(document).GetProperty("deactivateUser");
        var timestamp = actualPayload.GetProperty("user").GetProperty("deactivated").GetDateTime();
        var expectedPayload = new
        {
            Deactivated = true,
            User = new
            {
                user.Id,
                user.Name,
                Modified = timestamp,
                Deactivated = timestamp,
                Revision = user.Revision + 1,
            },
            Errors = (object?)null,
        };
        JsonAssert.Equivalent(expectedPayload, actualPayload);
    }

    [Fact]
    public async Task DeactivatingUserWhenNotAnAdmin()
    {
        var user = this.ModelFactory.BuildUser();
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = false };
        await this.DatabaseFixture.InsertEntities(user, currentUser);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await PostDeactivateUserMutation(client, user.Id);
        using var document = await response.Content.ReadAsJsonDocument();

        JsonAssert.ValueIsNull(document.RootElement.GetProperty("data"));
        ApiAssert.HasSingleError(ErrorCodes.Authentication.NotAuthorized, document);
    }

    [Fact]
    public async Task DeactivatingAlreadyDeactivatedUser()
    {
        var user = this.ModelFactory.BuildUser(deactivated: true);
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = true };
        await this.DatabaseFixture.InsertEntities(user, currentUser);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await PostDeactivateUserMutation(client, user.Id);
        using var document = await response.Content.ReadAsJsonDocument();

        var actualPayload = ApiAssert.SuccessResponse(document).GetProperty("deactivateUser");
        var expectedPayload = new
        {
            Deactivated = false,
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
    public async Task DeactivatingNonExistentUser()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = true };
        await this.DatabaseFixture.InsertEntities(currentUser);

        var nonExistentUserId = this.ModelFactory.NextInt();

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await PostDeactivateUserMutation(client, nonExistentUserId);
        using var document = await response.Content.ReadAsJsonDocument();

        var actualPayload = ApiAssert.SuccessResponse(document).GetProperty("deactivateUser");
        var expectedPayload = new
        {
            Deactivated = (bool?)null,
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

    private static Task<HttpResponseMessage> PostDeactivateUserMutation(
        HttpClient client, long id) =>
        client.PostQuery("""
            mutation($id: Long!) {
                deactivateUser(input: { id: $id }) {
                    deactivated
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
