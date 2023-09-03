using System.Security.Cryptography;
using Buttercup.DataAccess;
using Buttercup.EntityModel;
using Buttercup.TestUtils;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Buttercup.Security;

public sealed class TokenAuthenticationServiceTests
{
    #region IssueAccessToken

    [Fact]
    public async Task IssueAccessTokenEmitsLogMessage()
    {
        using var fixture = new IssueAccessTokenFixture();

        await fixture.IssueAccessToken();

        Assert.Contains(
            fixture.Logger.Entries,
            entry =>
                entry.LogLevel == LogLevel.Information &&
                entry.Message == $"Issued access token for user {fixture.User.Id} ({fixture.User.Email})");
    }

    [Fact]
    public async Task IssueAccessTokenLogsAuthenticationEvent()
    {
        using var fixture = new IssueAccessTokenFixture();

        await fixture.IssueAccessToken();

        fixture.MockAuthenticationEventDataProvider.Verify(x => x.LogEvent(
            fixture.DbContextFactory.FakeDbContext, "access_token_issued", fixture.User.Id, null));
    }

    [Fact]
    public async Task IssueAccessTokenReturnsToken()
    {
        using var fixture = new IssueAccessTokenFixture();

        Assert.Equal(fixture.AccessToken, await fixture.IssueAccessToken());
    }

    private sealed class IssueAccessTokenFixture : TokenAuthenticationServiceFixture
    {
        public IssueAccessTokenFixture() =>
            this.MockAccessTokenEncoder
                .Setup(x => x.Encode(new(this.User.Id, this.User.SecurityStamp, this.Clock.UtcNow)))
                .Returns(this.AccessToken);

        public User User { get; } = new ModelFactory().BuildUser();

        public string AccessToken { get; } = "sample-access-token";

        public Task<string> IssueAccessToken() =>
            this.TokenAuthenticationService.IssueAccessToken(this.User);
    }

    #endregion

    #region ValidateAccessToken

    [Fact]
    public async Task ValidateAccessTokenLogsAndReturnsNullWhenTokenIsNotBase64UrlEncoded()
    {
        using var fixture = new ValidateAccessTokenFixture();

        fixture.SetupDecodeFailure(new FormatException());

        Assert.Null(await fixture.ValidateAccessToken());

        Assert.Contains(
            fixture.Logger.Entries,
            entry =>
                entry.LogLevel == LogLevel.Warning &&
                entry.Message == "Access token failed validation; not base64url encoded");
    }

    [Fact]
    public async Task ValidateAccessTokenLogsAndReturnsNullWhenTokenIsMalformed()
    {
        using var fixture = new ValidateAccessTokenFixture();

        fixture.SetupDecodeFailure(new CryptographicException());

        Assert.Null(await fixture.ValidateAccessToken());

        Assert.Contains(
            fixture.Logger.Entries,
            entry =>
                entry.LogLevel == LogLevel.Warning &&
                entry.Message == "Access token failed validation; malformed or encrypted with wrong key");
    }

    [Fact]
    public async Task ValidateAccessTokenLogsAndReturnsNullWhenTokenHasExpired()
    {
        using var fixture = new ValidateAccessTokenFixture();

        fixture.SetupDecodeSuccess(tokenAge: new(24, 0, 1));

        Assert.Null(await fixture.ValidateAccessToken());

        Assert.Contains(
            fixture.Logger.Entries,
            entry =>
                entry.LogLevel == LogLevel.Information &&
                entry.Message == $"Access token failed validation for user {fixture.User.Id}; expired");
    }

    [Fact]
    public async Task ValidateAccessTokenLogsAndReturnsNullWhenUserDoesNotExist()
    {
        using var fixture = new ValidateAccessTokenFixture();

        fixture.SetupDecodeSuccess();
        fixture.SetupUserNotFound();

        Assert.Null(await fixture.ValidateAccessToken());

        Assert.Contains(
            fixture.Logger.Entries,
            entry =>
                entry.LogLevel == LogLevel.Warning &&
                entry.Message == $"Access token failed validation for user {fixture.User.Id}; user does not exist");
    }

    [Fact]
    public async Task ValidateAccessTokenLogsAndReturnsNullWhenSecurityStampHasChanged()
    {
        using var fixture = new ValidateAccessTokenFixture();

        fixture.SetupDecodeSuccess(securityStamp: "stale-security-stamp");
        fixture.SetupGetUserSuccess();

        Assert.Null(await fixture.ValidateAccessToken());

        Assert.Contains(
            fixture.Logger.Entries,
            entry =>
                entry.LogLevel == LogLevel.Information &&
                entry.Message == $"Access token failed validation for user {fixture.User.Id}; contains stale security stamp");
    }

    [Fact]
    public async Task ValidateAccessTokenLogsAndReturnsUserOnSuccess()
    {
        using var fixture = new ValidateAccessTokenFixture();

        fixture.SetupDecodeSuccess();
        fixture.SetupGetUserSuccess();

        Assert.Equal(fixture.User, await fixture.ValidateAccessToken());

        Assert.Contains(
            fixture.Logger.Entries,
            entry =>
                entry.LogLevel == LogLevel.Information &&
                entry.Message == $"Access token successfully validated for user {fixture.User.Id}");
    }

    private sealed class ValidateAccessTokenFixture : TokenAuthenticationServiceFixture
    {
        private const string AccessToken = "sample-access-token";

        public User User { get; } = new ModelFactory().BuildUser();

        public void SetupDecodeFailure(Exception exception) =>
            this.MockAccessTokenEncoder
                .Setup(x => x.Decode(AccessToken))
                .Throws(exception);

        public void SetupDecodeSuccess(TimeSpan? tokenAge = null, string? securityStamp = null)
        {
            var accessTokenPayload = new AccessTokenPayload(
                this.User.Id,
                securityStamp ?? this.User.SecurityStamp,
                this.Clock.UtcNow.Subtract(tokenAge ?? new(24, 0, 0)));

            this.MockAccessTokenEncoder
                .Setup(x => x.Decode(AccessToken))
                .Returns(accessTokenPayload);
        }

        public void SetupUserNotFound() =>
            this.MockUserDataProvider
                .Setup(x => x.GetUser(this.DbContextFactory.FakeDbContext, this.User.Id))
                .ThrowsAsync(new NotFoundException(string.Empty));

        public void SetupGetUserSuccess() =>
            this.MockUserDataProvider
                .Setup(x => x.GetUser(this.DbContextFactory.FakeDbContext, this.User.Id))
                .ReturnsAsync(this.User);

        public Task<User?> ValidateAccessToken() =>
            this.TokenAuthenticationService.ValidateAccessToken(AccessToken);
    }

    #endregion

    private class TokenAuthenticationServiceFixture : IDisposable
    {
        public TokenAuthenticationServiceFixture() =>
            this.TokenAuthenticationService = new(
                this.MockAccessTokenEncoder.Object,
                this.MockAuthenticationEventDataProvider.Object,
                this.Clock,
                this.DbContextFactory,
                this.Logger,
                this.MockUserDataProvider.Object);

        public StoppedClock Clock { get; } = new();

        public FakeDbContextFactory DbContextFactory { get; } = new();

        public Mock<IAccessTokenEncoder> MockAccessTokenEncoder { get; } = new();

        public Mock<IAuthenticationEventDataProvider> MockAuthenticationEventDataProvider { get; }
            = new();

        public Mock<IUserDataProvider> MockUserDataProvider { get; } = new();

        public ListLogger<TokenAuthenticationService> Logger { get; } = new();

        public TokenAuthenticationService TokenAuthenticationService { get; }

        public void Dispose() => this.DbContextFactory.Dispose();
    }
}
