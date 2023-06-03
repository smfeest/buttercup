using Buttercup.EntityModel;
using Buttercup.Web.Authentication;
using Buttercup.Web.TestUtils;
using Microsoft.AspNetCore.Identity;
using Xunit;

namespace Buttercup.Web.Api;

public class AuthenticateTests : EndToEndTests<AuthenticateTests>
{
    private const string UserEmail = "user@example.com";
    private const string UserPassword = "secret-password";

    public AuthenticateTests(AppFactory<AuthenticateTests> appFactory) : base(appFactory)
    {
    }

    [Fact]
    public async Task AuthenticatingSuccessfully()
    {
        await InsertUser();

        using var client = this.AppFactory.CreateClient();
        using var response = await PostAuthenticateMutation(client, new(UserEmail, UserPassword));
        using var document = await response.Content.ReadAsJsonDocument();

        var dataElement = ApiAssert.SuccessResponse(document);
        var authenticateElement = dataElement.GetProperty("authenticate");

        Assert.True(
            authenticateElement.GetProperty("isSuccess").GetBoolean());

        Assert.Equal(
            UserEmail,
            authenticateElement.GetProperty("user").GetProperty("email").GetString());

        var accessToken = authenticateElement.GetProperty("accessToken").GetString();
        Assert.NotNull(accessToken);

        var authenticatedUser =
            await this.AppFactory.Services
                .GetRequiredService<ITokenAuthenticationService>()
                .ValidateAccessToken(accessToken);

        Assert.NotNull(authenticatedUser);
        Assert.Equal(UserEmail, authenticatedUser.Email);
    }

    [Fact]
    public async Task FailingToAuthenticateDueToIncorrectPassword()
    {
        await InsertUser();

        using var client = this.AppFactory.CreateClient();
        using var response = await PostAuthenticateMutation(
            client, new(UserEmail, "incorrect-password"));
        using var document = await response.Content.ReadAsJsonDocument();

        var dataElement = ApiAssert.SuccessResponse(document);

        JsonAssert.Equals(new AuthenticatePayload(false), dataElement.GetProperty("authenticate"));
    }

    private async Task InsertUser()
    {
        var userPasswordHasher =
            this.AppFactory.Services.GetRequiredService<IPasswordHasher<User>>();

        var user = this.ModelFactory.BuildUser() with { Email = UserEmail };
        user.HashedPassword = userPasswordHasher.HashPassword(user, UserPassword);

        await this.DatabaseFixture.InsertEntities(user);
    }

    private static Task<HttpResponseMessage> PostAuthenticateMutation(
        HttpClient client, AuthenticateInput input) =>
        client.PostQuery(
            @"mutation($input: AuthenticateInput!) {
                authenticate(input: $input) {
                    isSuccess
                    accessToken
                    user {
                        email
                    }
                }
            }",
            new { input });
}