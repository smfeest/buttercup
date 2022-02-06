using Buttercup.Models;
using Buttercup.TestUtils;
using Buttercup.Web.Authentication;
using Moq;
using Xunit;

namespace Buttercup.Web.Api;

public class MutationTests
{
    #region CurrentUser

    [Fact]
    public async Task AuthenticateReturnsPayloadWithSuccessFlagAccessTokenAndUserOnSuccess()
    {
        var fixture = new AuthenticateFixture();

        Assert.Equal(
            new(true, fixture.AccessToken, fixture.User),
            await fixture.Authenticate(true));
    }

    [Fact]
    public async Task AuthenticateReturnsPayloadWithNoAccessTokenOrUserOnFailure() =>
        Assert.Equal(new(false, null, null), await new AuthenticateFixture().Authenticate(false));

    private class AuthenticateFixture
    {
        public string AccessToken { get; } = "access-token";

        public User User { get; } = ModelFactory.CreateUser();

        public async Task<AuthenticatePayload> Authenticate(bool success)
        {
            const string Email = "user@example.com";
            const string Password = "user-password";

            var authenticationManager = Mock.Of<IAuthenticationManager>(
                x => x.Authenticate(Email, Password) == Task.FromResult(
                    success ? this.User : null));

            var tokenAuthenticationService = Mock.Of<ITokenAuthenticationService>(
                x => x.IssueAccessToken(this.User) == Task.FromResult(this.AccessToken));

            return await new Mutation().Authenticate(
                authenticationManager, tokenAuthenticationService, new(Email, Password));
        }
    }

    #endregion
}
