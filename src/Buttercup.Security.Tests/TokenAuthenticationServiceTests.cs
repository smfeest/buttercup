using System.Security.Cryptography;
using Buttercup.DataAccess;
using Buttercup.EntityModel;
using Buttercup.TestUtils;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Buttercup.Security;

public sealed class TokenAuthenticationServiceTests : IDisposable
{
    private readonly ModelFactory modelFactory = new();

    private readonly Mock<IAccessTokenEncoder> accessTokenEncoderMock = new();
    private readonly Mock<IAuthenticationEventDataProvider> authenticationEventDataProviderMock =
        new();
    private readonly StoppedClock clock = new();
    private readonly FakeDbContextFactory dbContextFactory = new();
    private readonly ListLogger<TokenAuthenticationService> logger = new();
    private readonly Mock<IUserDataProvider> userDataProviderMock = new();

    private readonly TokenAuthenticationService tokenAuthenticationService;

    public TokenAuthenticationServiceTests() =>
        this.tokenAuthenticationService = new(
            this.accessTokenEncoderMock.Object,
            this.authenticationEventDataProviderMock.Object,
            this.clock,
            this.dbContextFactory,
            this.logger,
            this.userDataProviderMock.Object);

    public void Dispose() => this.dbContextFactory.Dispose();

    #region IssueAccessToken

    [Fact]
    public async Task IssueAccessToken_EmitsLogMessage()
    {
        var user = this.modelFactory.BuildUser();

        this.SetupEncode(user, this.modelFactory.NextString("access-token"));

        await this.tokenAuthenticationService.IssueAccessToken(user);

        LogAssert.HasEntry(
            this.logger,
            LogLevel.Information,
            $"Issued access token for user {user.Id} ({user.Email})");
    }

    [Fact]
    public async Task IssueAccessToken_LogsAuthenticationEvent()
    {
        var user = this.modelFactory.BuildUser();

        this.SetupEncode(user, this.modelFactory.NextString("access-token"));

        await this.tokenAuthenticationService.IssueAccessToken(user);

        this.authenticationEventDataProviderMock.Verify(x => x.LogEvent(
            this.dbContextFactory.FakeDbContext, "access_token_issued", user.Id, null));
    }

    [Fact]
    public async Task IssueAccessToken_ReturnsToken()
    {
        var user = this.modelFactory.BuildUser();
        var expectedToken = this.modelFactory.NextString("access-token");

        this.SetupEncode(user, expectedToken);

        var actualToken = await this.tokenAuthenticationService.IssueAccessToken(user);

        Assert.Equal(expectedToken, actualToken);
    }

    private void SetupEncode(User user, string accessToken) =>
        this.accessTokenEncoderMock
            .Setup(x => x.Encode(new(user.Id, user.SecurityStamp, this.clock.UtcNow)))
            .Returns(accessToken);

    #endregion

    #region ValidateAccessToken

    [Fact]
    public async Task ValidateAccessToken_TokenIsNotBase64UrlEncoded_LogsAndReturnsNull()
    {
        var accessToken = this.modelFactory.NextString("access-token");

        this.SetupDecodeFailure(accessToken, new FormatException());

        Assert.Null(await this.tokenAuthenticationService.ValidateAccessToken(accessToken));

        LogAssert.HasEntry(
            this.logger, LogLevel.Warning, "Access token failed validation; not base64url encoded");
    }

    [Fact]
    public async Task ValidateAccessToken_TokenIsMalformed_LogsAndReturnsNull()
    {
        var accessToken = this.modelFactory.NextString("access-token");

        this.SetupDecodeFailure(accessToken, new CryptographicException());

        Assert.Null(await this.tokenAuthenticationService.ValidateAccessToken(accessToken));

        LogAssert.HasEntry(
            this.logger,
            LogLevel.Warning,
            "Access token failed validation; malformed or encrypted with wrong key");
    }

    [Fact]
    public async Task ValidateAccessToken_TokenHasExpired_LogsAndReturnsNull()
    {
        var accessToken = this.modelFactory.NextString("access-token");
        var user = this.modelFactory.BuildUser();

        this.SetupDecodeSuccess(accessToken, user, tokenAge: new(24, 0, 1));

        Assert.Null(await this.tokenAuthenticationService.ValidateAccessToken(accessToken));

        LogAssert.HasEntry(
            this.logger,
            LogLevel.Information,
            $"Access token failed validation for user {user.Id}; expired");
    }

    [Fact]
    public async Task ValidateAccessToken_UserDoesNotExist_LogsAndReturnsNull()
    {
        var accessToken = this.modelFactory.NextString("access-token");
        var user = this.modelFactory.BuildUser();

        this.SetupDecodeSuccess(accessToken, user);
        this.SetupUserNotFound(user);

        Assert.Null(await this.tokenAuthenticationService.ValidateAccessToken(accessToken));

        LogAssert.HasEntry(
            this.logger,
            LogLevel.Warning,
            $"Access token failed validation for user {user.Id}; user does not exist");
    }

    [Fact]
    public async Task ValidateAccessToken_SecurityStampHasChanged_LogsAndReturnsNull()
    {
        var accessToken = this.modelFactory.NextString("access-token");
        var user = this.modelFactory.BuildUser();

        this.SetupDecodeSuccess(accessToken, user, securityStamp: "stale-security-stamp");
        this.SetupGetUserSuccess(user);

        Assert.Null(await this.tokenAuthenticationService.ValidateAccessToken(accessToken));

        LogAssert.HasEntry(
            this.logger,
            LogLevel.Information,
            $"Access token failed validation for user {user.Id}; contains stale security stamp");
    }

    [Fact]
    public async Task ValidateAccessToken_Success_LogsAndReturnsUser()
    {
        var accessToken = this.modelFactory.NextString("access-token");
        var user = this.modelFactory.BuildUser();

        this.SetupDecodeSuccess(accessToken, user);
        this.SetupGetUserSuccess(user);

        Assert.Equal(user, await this.tokenAuthenticationService.ValidateAccessToken(accessToken));

        LogAssert.HasEntry(
            this.logger,
            LogLevel.Information,
            $"Access token successfully validated for user {user.Id}");
    }

    private void SetupDecodeFailure(string accessToken, Exception exception) =>
        this.accessTokenEncoderMock
            .Setup(x => x.Decode(accessToken))
            .Throws(exception);

    private void SetupDecodeSuccess(
        string accessToken, User user, TimeSpan? tokenAge = null, string? securityStamp = null)
    {
        var accessTokenPayload = new AccessTokenPayload(
            user.Id,
            securityStamp ?? user.SecurityStamp,
            this.clock.UtcNow.Subtract(tokenAge ?? new(24, 0, 0)));

        this.accessTokenEncoderMock
            .Setup(x => x.Decode(accessToken))
            .Returns(accessTokenPayload);
    }

    private void SetupUserNotFound(User user) =>
        this.userDataProviderMock
            .Setup(x => x.GetUser(this.dbContextFactory.FakeDbContext, user.Id))
            .ThrowsAsync(new NotFoundException(string.Empty));

    private void SetupGetUserSuccess(User user) =>
        this.userDataProviderMock
            .Setup(x => x.GetUser(this.dbContextFactory.FakeDbContext, user.Id))
            .ReturnsAsync(user);

    #endregion
}
