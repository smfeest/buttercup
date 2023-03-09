using System.Security.Cryptography;
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

    private sealed class IssueAccessTokenFixture : TokenAuthenticationServiceFixture
    {
        public IssueAccessTokenFixture() =>
            this.MockAccessTokenEncoder
                .Setup(x => x.Encode(new(this.User.Id, this.User.SecurityStamp, this.UtcNow)))
                .Returns(this.AccessToken);

        public User User { get; } = new ModelFactory().CreateUser();

        public string AccessToken { get; } = "sample-access-token";

        public Task<string> IssueAccessToken() =>
            this.TokenAuthenticationService.IssueAccessToken(this.User);
    }

    #endregion

    #region ValidateAccessToken

    [Fact]
    public async Task ValidateAccessTokenLogsAndReturnsNullWhenTokenIsNotBase64UrlEncoded()
    {
        var fixture = new ValidateAccessTokenFixture();

        fixture.SetupDecodeFailure(new FormatException());

        Assert.Null(await fixture.ValidateAccessToken());

        fixture.Logger.AssertSingleEntry(
            LogLevel.Warning, "Access token failed validation; not base64url encoded");
    }

    [Fact]
    public async Task ValidateAccessTokenLogsAndReturnsNullWhenTokenIsMalformed()
    {
        var fixture = new ValidateAccessTokenFixture();

        fixture.SetupDecodeFailure(new CryptographicException());

        Assert.Null(await fixture.ValidateAccessToken());

        fixture.Logger.AssertSingleEntry(
            LogLevel.Warning,
            "Access token failed validation; malformed or encrypted with wrong key");
    }

    [Fact]
    public async Task ValidateAccessTokenLogsAndReturnsNullWhenTokenHasExpired()
    {
        var fixture = new ValidateAccessTokenFixture();

        fixture.SetupDecodeSuccess(tokenAge: new(24, 0, 1));

        Assert.Null(await fixture.ValidateAccessToken());

        fixture.Logger.AssertSingleEntry(
            LogLevel.Information,
            $"Access token failed validation for user {fixture.User.Id}; expired");
    }

    [Fact]
    public async Task ValidateAccessTokenLogsAndReturnsNullWhenUserDoesNotExist()
    {
        var fixture = new ValidateAccessTokenFixture();

        fixture.SetupDecodeSuccess();
        fixture.SetupUserNotFound();

        Assert.Null(await fixture.ValidateAccessToken());

        fixture.Logger.AssertSingleEntry(
            LogLevel.Warning,
            $"Access token failed validation for user {fixture.User.Id}; user does not exist");
    }

    [Fact]
    public async Task ValidateAccessTokenLogsAndReturnsNullWhenSecurityStampHasChanged()
    {
        var fixture = new ValidateAccessTokenFixture();

        fixture.SetupDecodeSuccess(securityStamp: "stale-security-stamp");
        fixture.SetupGetUserSuccess();

        Assert.Null(await fixture.ValidateAccessToken());

        fixture.Logger.AssertSingleEntry(
            LogLevel.Information,
            $"Access token failed validation for user {fixture.User.Id}; contains stale security stamp");
    }

    [Fact]
    public async Task ValidateAccessTokenLogsAndReturnsUserOnSuccess()
    {
        var fixture = new ValidateAccessTokenFixture();

        fixture.SetupDecodeSuccess();
        fixture.SetupGetUserSuccess();

        Assert.Equal(fixture.User, await fixture.ValidateAccessToken());

        fixture.Logger.AssertSingleEntry(
            LogLevel.Information, $"Access token successfully validated for user {fixture.User.Id}");
    }

    private sealed class ValidateAccessTokenFixture : TokenAuthenticationServiceFixture
    {
        private const string AccessToken = "sample-access-token";

        public User User { get; } = new ModelFactory().CreateUser();

        public void SetupDecodeFailure(Exception exception) =>
            this.MockAccessTokenEncoder
                .Setup(x => x.Decode(AccessToken))
                .Throws(exception);

        public void SetupDecodeSuccess(TimeSpan? tokenAge = null, string? securityStamp = null)
        {
            var accessTokenPayload = new AccessTokenPayload(
                this.User.Id,
                securityStamp ?? this.User.SecurityStamp,
                this.UtcNow.Subtract(tokenAge ?? new(24, 0, 0)));

            this.MockAccessTokenEncoder
                .Setup(x => x.Decode(AccessToken))
                .Returns(accessTokenPayload);
        }

        public void SetupUserNotFound() =>
            this.MockUserDataProvider
                .Setup(x => x.GetUser(this.MySqlConnection, this.User.Id))
                .ThrowsAsync(new NotFoundException(string.Empty));

        public void SetupGetUserSuccess() =>
            this.MockUserDataProvider
                .Setup(x => x.GetUser(this.MySqlConnection, this.User.Id))
                .ReturnsAsync(this.User);

        public Task<User?> ValidateAccessToken() =>
            this.TokenAuthenticationService.ValidateAccessToken(AccessToken);
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
                mySqlConnectionSource,
                this.MockUserDataProvider.Object);
        }

        public Mock<IAccessTokenEncoder> MockAccessTokenEncoder { get; } = new();

        public Mock<IAuthenticationEventDataProvider> MockAuthenticationEventDataProvider { get; }
            = new();

        public Mock<IUserDataProvider> MockUserDataProvider { get; } = new();

        public ListLogger<TokenAuthenticationService> Logger { get; } = new();

        public MySqlConnection MySqlConnection { get; } = new();

        public TokenAuthenticationService TokenAuthenticationService { get; }

        public DateTime UtcNow { get; } = DateTime.UtcNow;
    }
}
