using Buttercup.DataAccess;
using Buttercup.Models;
using Buttercup.TestUtils;
using Buttercup.Web.TestUtils;
using Microsoft.Extensions.Logging;
using Moq;
using MySqlConnector;
using Xunit;

namespace Buttercup.Web.Authentication;

public class TokenAuthenticationServiceTests
{
    #region IssueAccessToken

    [Fact]
    public async Task IssueAccessTokenEmitsLogMessage()
    {
        var fixture = new IssueAccessTokenFixture();

        await fixture.IssueAccessToken();

        fixture.Logger.AssertSingleEntry(
            LogLevel.Information,
            $"Issued access token for user {fixture.User.Id} ({fixture.User.Email})");
    }

    [Fact]
    public async Task IssueAccessTokenLogsAuthenticationEvent()
    {
        var fixture = new IssueAccessTokenFixture();

        await fixture.IssueAccessToken();

        fixture.MockAuthenticationEventDataProvider.Verify(x => x.LogEvent(
            fixture.MySqlConnection, "access_token_issued", fixture.User.Id, null));
    }

    [Fact]
    public async Task IssueAccessTokenReturnsToken()
    {
        var fixture = new IssueAccessTokenFixture();

        Assert.Equal(fixture.AccessToken, await fixture.IssueAccessToken());
    }

    private class IssueAccessTokenFixture : TokenAuthenticationServiceFixture
    {
        public IssueAccessTokenFixture() =>
            this.MockAccessTokenEncoder
                .Setup(x => x.Encode(new(this.User.Id, this.User.SecurityStamp, this.UtcNow)))
                .Returns(this.AccessToken);

        public User User { get; } = ModelFactory.CreateUser();

        public string AccessToken { get; } = "sample-access-token";

        public Task<string> IssueAccessToken() =>
            this.TokenAuthenticationService.IssueAccessToken(this.User);
    }

    #endregion

    private class TokenAuthenticationServiceFixture
    {
        public TokenAuthenticationServiceFixture()
        {
            var clock = Mock.Of<IClock>(x => x.UtcNow == this.UtcNow);
            var mySqlConnectionSource = Mock.Of<IMySqlConnectionSource>(
                x => x.OpenConnection() == Task.FromResult(this.MySqlConnection));

            this.TokenAuthenticationService = new(
                this.MockAccessTokenEncoder.Object,
                this.MockAuthenticationEventDataProvider.Object,
                clock,
                this.Logger,
                mySqlConnectionSource);
        }

        public Mock<IAccessTokenEncoder> MockAccessTokenEncoder { get; } = new();

        public Mock<IAuthenticationEventDataProvider> MockAuthenticationEventDataProvider { get; }
            = new();

        public ListLogger<TokenAuthenticationService> Logger { get; } = new();

        public MySqlConnection MySqlConnection { get; } = new();

        public TokenAuthenticationService TokenAuthenticationService { get; }

        public DateTime UtcNow { get; } = DateTime.UtcNow;
    }
}
