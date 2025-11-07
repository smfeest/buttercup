using Buttercup.Security;
using Buttercup.Web.TestUtils;
using Xunit;

namespace Buttercup.Web.Api;

public sealed class CreateTestUserTests(AppFactory appFactory) : EndToEndTests(appFactory)
{
    [Fact]
    public async Task CreatingTestUser()
    {
        var currentUser = this.ModelFactory.BuildUser() with { IsAdmin = true };
        await this.DatabaseFixture.InsertEntities(currentUser);

        using var client = await this.AppFactory.CreateClientForApiUser(currentUser);

        using var response = await client.PostQuery("""
            mutation {
                createTestUser {
                    user {
                        id
                        email
                    }
                    password
                }
            }
            """);
        using var document = await response.Content.ReadAsJsonDocument();

        var createUserElement = ApiAssert.SuccessResponse(document).GetProperty("createTestUser");
        var userElement = createUserElement.GetProperty("user");

        var passwordAuthenticationService =
            this.AppFactory.Services.GetRequiredService<IPasswordAuthenticationService>();

        var authenticationResult = await passwordAuthenticationService.Authenticate(
            userElement.GetProperty("email").GetString()!,
            createUserElement.GetProperty("password").GetString()!,
            null);

        Assert.True(authenticationResult.IsSuccess);
        Assert.Equal(userElement.GetProperty("id").GetInt64(), authenticationResult.User.Id);
    }
}
