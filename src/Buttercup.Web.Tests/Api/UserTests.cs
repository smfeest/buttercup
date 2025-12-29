using Buttercup.Web.TestUtils;
using HotChocolate;
using Xunit;

namespace Buttercup.Web.Api;

public sealed class UserTests(AppFactory appFactory) : EndToEndTests(appFactory)
{
    [Fact]
    public async Task QueryingPublicFieldsOnAnotherUserWhenNotAnAdmin()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = false };
        var user = this.ModelFactory.BuildUser(setOptionalAttributes: true, deactivated: true);
        await this.DatabaseFixture.InsertEntities(currentUser, user);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await PostPublicFieldsQuery(client, user.Id);
        using var document = await response.Content.ReadAsJsonDocument();

        var dataElement = ApiAssert.SuccessResponse(document);

        var expected = new
        {
            user.Id,
            user.Name,
            user.TimeZone,
            user.Created,
            user.Modified,
            user.Deactivated,
            user.Revision,
        };

        JsonAssert.Equivalent(expected, dataElement.GetProperty("user"));
    }

    [Fact]
    public async Task QueryingPrivateFieldsOnSelfWhenNotAnAdmin()
    {
        var user = this.ModelFactory.BuildUser() with { IsAdmin = false };
        await this.DatabaseFixture.InsertEntities(user);

        using var client = await this.AppFactory.CreateClientForApiUser(user);
        using var response = await PostPrivateFieldsQuery(client, user.Id);
        using var document = await response.Content.ReadAsJsonDocument();

        var dataElement = ApiAssert.SuccessResponse(document);

        var expected = new
        {
            user.Email,
            user.PasswordCreated,
            user.IsAdmin,
        };

        JsonAssert.Equivalent(expected, dataElement.GetProperty("user"));
    }

    [Fact]
    public async Task QueryingPrivateFieldsOnAnotherUserWhenNotAnAdmin()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = false };
        var user = this.ModelFactory.BuildUser();
        await this.DatabaseFixture.InsertEntities(currentUser, user);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await PostPrivateFieldsQuery(client, user.Id);
        using var document = await response.Content.ReadAsJsonDocument();

        JsonAssert.ValueIsNull(document.RootElement.GetProperty("data").GetProperty("user"));

        var expectedErrors = new[]
        {
            new
            {
                Path = new string[] { "user", "email" },
                Extensions = new { Code = ErrorCodes.Authentication.NotAuthorized },
            },
            new
            {
                Path = new string[] { "user", "passwordCreated" },
                Extensions = new { Code = ErrorCodes.Authentication.NotAuthorized },
            },
            new
            {
                Path = new string[] { "user", "isAdmin" },
                Extensions = new { Code = ErrorCodes.Authentication.NotAuthorized },
            },
        };

        Assert.True(document.RootElement.TryGetProperty("errors", out var errorsElement));
        JsonAssert.Equivalent(expectedErrors, errorsElement);
    }

    [Fact]
    public async Task QueryingPrivateFieldsOnAnotherUserWhenAnAdmin()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = true };
        var user = this.ModelFactory.BuildUser();
        await this.DatabaseFixture.InsertEntities(currentUser, user);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await PostPrivateFieldsQuery(client, user.Id);
        using var document = await response.Content.ReadAsJsonDocument();

        var dataElement = ApiAssert.SuccessResponse(document);

        var expected = new
        {
            user.Email,
            user.PasswordCreated,
            user.IsAdmin,
        };

        JsonAssert.Equivalent(expected, dataElement.GetProperty("user"));
    }

    [Fact]
    public async Task QueryingNonExistentUser()
    {
        var currentUser = this.ModelFactory.BuildUser();
        await this.DatabaseFixture.InsertEntities(currentUser);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);
        using var response = await PostPublicFieldsQuery(client, this.ModelFactory.NextInt());
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
        using var response = await PostPublicFieldsQuery(client, user.Id);
        using var document = await response.Content.ReadAsJsonDocument();

        JsonAssert.ValueIsNull(document.RootElement.GetProperty("data").GetProperty("user"));

        ApiAssert.HasSingleError(ErrorCodes.Authentication.NotAuthenticated, document);
    }

    private static Task<HttpResponseMessage> PostPublicFieldsQuery(HttpClient client, long id) =>
        client.PostQuery("""
            query($id: Long!) {
                user(id: $id) {
                    id
                    name
                    timeZone
                    created
                    modified
                    deactivated
                    revision
                }
            }
            """,
            new { id });

    private static Task<HttpResponseMessage> PostPrivateFieldsQuery(HttpClient client, long id) =>
        client.PostQuery("""
            query($id: Long!) {
                user(id: $id) {
                    email
                    passwordCreated
                    isAdmin
                }
            }
            """,
            new { id });
}
