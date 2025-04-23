using Buttercup.EntityModel;
using Buttercup.Security;
using Buttercup.Web.TestUtils;
using Microsoft.AspNetCore.Identity;
using Xunit;

namespace Buttercup.Web.Api;

public sealed class AuthenticateTests(AppFactory appFactory) : EndToEndTests(appFactory)
{
    private const string UserPassword = "secret-password";

    [Fact]
    public async Task AuthenticatingSuccessfully()
    {
        var user = await this.InsertUser();

        using var client = this.AppFactory.CreateClient();
        using var response = await PostAuthenticateMutation(client, user.Email, UserPassword);
        using var document = await response.Content.ReadAsJsonDocument();

        var dataElement = ApiAssert.SuccessResponse(document);
        var authenticateElement = dataElement.GetProperty("authenticate");

        JsonAssert.Equivalent(
            new
            {
                IsSuccess = true,
                User = new { user.Email },
                Errors = default(object?),
            },
            authenticateElement);

        var accessToken = authenticateElement.GetProperty("accessToken").GetString();
        Assert.NotNull(accessToken);

        var authenticatedUser =
            await this.AppFactory.Services
                .GetRequiredService<ITokenAuthenticationService>()
                .ValidateAccessToken(accessToken);

        Assert.NotNull(authenticatedUser);
        Assert.Equal(user.Email, authenticatedUser.Email);
    }

    [Fact]
    public async Task FailingToAuthenticateDueToIncorrectPassword()
    {
        var user = await this.InsertUser();

        using var client = this.AppFactory.CreateClient();
        using var response = await PostAuthenticateMutation(
            client, user.Email, "incorrect-password");
        using var document = await response.Content.ReadAsJsonDocument();

        var dataElement = ApiAssert.SuccessResponse(document);

        JsonAssert.Equivalent(
            new
            {
                IsSuccess = false,
                AccessToken = default(string?),
                Errors = new[]
                {
                    new
                    {
                        __typename = "IncorrectCredentialsError",
                        Message = "Wrong email address or password",
                    }
                },
            },
            dataElement.GetProperty("authenticate"));
    }

    [Fact]
    public async Task FailingToAuthenticateDueToTooManyFailedAttempts()
    {
        var user = await this.InsertUser();

        var rateLimiter =
            this.AppFactory.Services.GetRequiredService<IPasswordAuthenticationRateLimiter>();
        var maxAttempts =
            this.AppFactory.GetOptions<SecurityOptions>().PasswordAuthenticationRateLimit.Limit;

        for (var i = 0; i < maxAttempts; i++)
        {
            await rateLimiter.IsAllowed(user.Email);
        }

        using var client = this.AppFactory.CreateClient();
        using var response = await PostAuthenticateMutation(
            client, user.Email, "incorrect-password");
        using var document = await response.Content.ReadAsJsonDocument();

        var dataElement = ApiAssert.SuccessResponse(document);

        JsonAssert.Equivalent(
            new
            {
                IsSuccess = false,
                AccessToken = default(string?),
                Errors = new[]
                {
                    new
                    {
                        __typename = "TooManyAttemptsError",
                        Message = "Too many failed attempts. Reset your password or try again later.",
                    }
                },
            },
            dataElement.GetProperty("authenticate"));
    }

    private async Task<User> InsertUser()
    {
        var userPasswordHasher =
            this.AppFactory.Services.GetRequiredService<IPasswordHasher<User>>();

        var user = this.ModelFactory.BuildUser() with
        {
            Email = $"user{Random.Shared.Next()}@example.com"
        };
        user.HashedPassword = userPasswordHasher.HashPassword(user, UserPassword);

        await this.DatabaseFixture.InsertEntities(user);

        return user;
    }

    private static Task<HttpResponseMessage> PostAuthenticateMutation(
        HttpClient client, string email, string password) =>
        client.PostQuery("""
            mutation($input: AuthenticateInput!) {
                authenticate(input: $input) {
                    isSuccess
                    accessToken
                    user {
                        email
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
            new { input = new { email, password } });
}
