using Buttercup.EntityModel;
using Buttercup.Web.TestUtils;
using Xunit;

namespace Buttercup.Web.Api;

public sealed class UsersTests(AppFactory appFactory) : EndToEndTests(appFactory)
{
    private const string UsersQuery =
        @"query {
            users {
                id
                name
                email
                passwordCreated
                timeZone
                created
                modified
                revision
            }
        }";

    [Fact]
    public async Task QueryingUsers()
    {
        var currentUser = this.ModelFactory.BuildUser(true) with { IsAdmin = true };
        var otherUser = this.ModelFactory.BuildUser(false);
        await this.DatabaseFixture.InsertEntities(currentUser, otherUser);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await client.PostQuery(UsersQuery);
        using var document = await response.Content.ReadAsJsonDocument();

        var dataElement = ApiAssert.SuccessResponse(document);

        var expected = new User[] { currentUser, otherUser }.Select(user => new
        {
            user.Id,
            user.Name,
            user.Email,
            user.PasswordCreated,
            user.TimeZone,
            user.Created,
            user.Modified,
            user.Revision,
        });

        JsonAssert.Equivalent(expected, dataElement.GetProperty("users"));
    }

    [Fact]
    public async Task QueryingUsersWhenUnauthenticated()
    {
        using var client = this.AppFactory.CreateClient();
        using var response = await client.PostQuery(UsersQuery);
        using var document = await response.Content.ReadAsJsonDocument();

        JsonAssert.ValueIsNull(document.RootElement.GetProperty("data"));
        ApiAssert.HasSingleError("AUTH_NOT_AUTHORIZED", document);
    }
}
