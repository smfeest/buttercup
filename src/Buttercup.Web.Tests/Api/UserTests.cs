using Buttercup.Web.TestUtils;
using Xunit;

namespace Buttercup.Web.Api;

public sealed class UserTests(AppFactory<UserTests> appFactory)
    : EndToEndTests<UserTests>(appFactory)
{
    [Fact]
    public async Task QueryingUser()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = true };
        var user = this.ModelFactory.BuildUser();

        await this.DatabaseFixture.InsertEntities(currentUser, user);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await PostUserQuery(client, user.Id);
        using var document = await response.Content.ReadAsJsonDocument();

        var dataElement = ApiAssert.SuccessResponse(document);

        var expected = new
        {
            user.Id,
            user.Name,
            user.Email,
            user.PasswordCreated,
            user.TimeZone,
            user.Created,
            user.Modified,
            user.Revision,
        };

        JsonAssert.Equivalent(expected, dataElement.GetProperty("user"));
    }

    [Fact]
    public async Task QueryingNonExistentUser()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = true };

        await this.DatabaseFixture.InsertEntities(currentUser);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await PostUserQuery(client, this.ModelFactory.NextInt());
        using var document = await response.Content.ReadAsJsonDocument();

        var dataElement = ApiAssert.SuccessResponse(document);

        JsonAssert.ValueIsNull(dataElement.GetProperty("user"));
    }

    [Fact]
    public async Task QueryingUserWhenUnauthenticated()
    {
        var user = this.ModelFactory.BuildUser();

        await this.DatabaseFixture.InsertEntities(user);

        using var client = this.AppFactory.CreateClient();
        using var response = await PostUserQuery(client, user.Id);
        using var document = await response.Content.ReadAsJsonDocument();

        JsonAssert.ValueIsNull(document.RootElement.GetProperty("data").GetProperty("user"));

        ApiAssert.HasSingleError("AUTH_NOT_AUTHORIZED", document);
    }

    private static Task<HttpResponseMessage> PostUserQuery(HttpClient client, long id) =>
        client.PostQuery(
            @"query($id: Long!) {
                user(id: $id) {
                    id
                    name
                    email
                    passwordCreated
                    timeZone
                    created
                    modified
                    revision
                }
            }",
            new { id });
}
