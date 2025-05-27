using System.Net;
using System.Security.Cryptography;
using Buttercup.EntityModel;
using Buttercup.TestUtils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Xunit;

namespace Buttercup.Security;

[Collection(nameof(DatabaseCollection))]
public sealed class TokenAuthenticationServiceTests : DatabaseTests<DatabaseCollection>
{
    private readonly ModelFactory modelFactory = new();

    private readonly Mock<IAccessTokenEncoder> accessTokenEncoderMock = new();
    private readonly FakeLogger<TokenAuthenticationService> logger = new();
    private readonly FakeTimeProvider timeProvider;

    private readonly TokenAuthenticationService tokenAuthenticationService;

    public TokenAuthenticationServiceTests(DatabaseFixture<DatabaseCollection> databaseFixture)
        : base(databaseFixture)
    {
        this.timeProvider = new(this.modelFactory.NextDateTime());

        this.tokenAuthenticationService = new(
            this.accessTokenEncoderMock.Object,
            this.DatabaseFixture,
            this.logger,
            this.timeProvider);
    }

    #region IssueAccessToken

    [Fact]
    public async Task IssueAccessToken()
    {
        var accessToken = this.modelFactory.NextString("access-token");
        var ipAddress = new IPAddress(this.modelFactory.NextInt());
        var user = this.modelFactory.BuildUser();

        await this.DatabaseFixture.InsertEntities(user);

        this.accessTokenEncoderMock
            .Setup(x => x.Encode(
                new(user.Id, user.SecurityStamp, this.timeProvider.GetUtcDateTimeNow())))
            .Returns(accessToken);

        var returnedToken = await this.tokenAuthenticationService.IssueAccessToken(user, ipAddress);

        using var dbContext = this.DatabaseFixture.CreateDbContext();

        // Inserts security event
        Assert.True(
            await dbContext.SecurityEvents.AnyAsync(
                securityEvent =>
                    securityEvent.Time == this.timeProvider.GetUtcDateTimeNow() &&
                    securityEvent.Event == "access_token_issued" &&
                    securityEvent.IpAddress == ipAddress &&
                    securityEvent.UserId == user.Id,
                TestContext.Current.CancellationToken));

        // Logs token issued message
        LogAssert.SingleEntry(this.logger)
            .HasId(300)
            .HasLevel(LogLevel.Information)
            .HasMessage($"Issued access token for user {user.Id} ({user.Email})");

        // Returns token
        Assert.Equal(accessToken, returnedToken);
    }

    #endregion

    #region ValidateAccessToken

    [Fact]
    public async Task ValidateAccessToken_TokenIsNotBase64UrlEncoded()
    {
        var accessToken = this.modelFactory.NextString("access-token");
        var exception = new FormatException();

        this.SetupDecodeFailure(accessToken, exception);

        // Returns null
        Assert.Null(await this.tokenAuthenticationService.ValidateAccessToken(accessToken));

        // Logs incorrect encoding message
        LogAssert.SingleEntry(this.logger)
            .HasId(301)
            .HasLevel(LogLevel.Warning)
            .HasMessage("Access token failed validation; not base64url encoded")
            .HasException(exception);
    }

    [Fact]
    public async Task ValidateAccessToken_TokenIsMalformed()
    {
        var accessToken = this.modelFactory.NextString("access-token");
        var exception = new CryptographicException();

        this.SetupDecodeFailure(accessToken, exception);

        // Returns null
        Assert.Null(await this.tokenAuthenticationService.ValidateAccessToken(accessToken));

        // Logs malformed message
        LogAssert.SingleEntry(this.logger)
            .HasId(302)
            .HasLevel(LogLevel.Warning)
            .HasMessage("Access token failed validation; malformed or encrypted with wrong key")
            .HasException(exception);
    }

    [Fact]
    public async Task ValidateAccessToken_TokenHasExpired()
    {
        var accessToken = this.modelFactory.NextString("access-token");
        var user = this.modelFactory.BuildUser();

        this.SetupDecodeSuccess(accessToken, user, tokenAge: new(24, 0, 1));

        // Returns null
        Assert.Null(await this.tokenAuthenticationService.ValidateAccessToken(accessToken));

        // Logs expired message
        LogAssert.SingleEntry(this.logger)
            .HasId(303)
            .HasLevel(LogLevel.Information)
            .HasMessage($"Access token failed validation for user {user.Id}; expired");
    }

    [Fact]
    public async Task ValidateAccessToken_UserDoesNotExist()
    {
        var accessToken = this.modelFactory.NextString("access-token");
        var user = this.modelFactory.BuildUser();

        this.SetupDecodeSuccess(accessToken, user);

        // Returns null
        Assert.Null(await this.tokenAuthenticationService.ValidateAccessToken(accessToken));

        // Logs user does not exist message
        LogAssert.SingleEntry(this.logger)
            .HasId(304)
            .HasLevel(LogLevel.Warning)
            .HasMessage($"Access token failed validation for user {user.Id}; user does not exist");
    }

    [Fact]
    public async Task ValidateAccessToken_SecurityStampHasChanged()
    {
        var accessToken = this.modelFactory.NextString("access-token");
        var user = this.modelFactory.BuildUser();

        await this.DatabaseFixture.InsertEntities(user);

        this.SetupDecodeSuccess(accessToken, user, securityStamp: "stale-security-stamp");

        // Returns null
        Assert.Null(await this.tokenAuthenticationService.ValidateAccessToken(accessToken));

        // Logs stale security stamp message
        LogAssert.SingleEntry(this.logger)
            .HasId(305)
            .HasLevel(LogLevel.Information)
            .HasMessage(
                $"Access token failed validation for user {user.Id}; contains stale security stamp");
    }

    [Fact]
    public async Task ValidateAccessToken_Success()
    {
        var accessToken = this.modelFactory.NextString("access-token");
        var user = this.modelFactory.BuildUser();

        await this.DatabaseFixture.InsertEntities(user);

        this.SetupDecodeSuccess(accessToken, user);

        // Returns user
        Assert.Equal(user, await this.tokenAuthenticationService.ValidateAccessToken(accessToken));

        // Logs successfully validated message
        LogAssert.SingleEntry(this.logger)
            .HasId(306)
            .HasLevel(LogLevel.Information)
            .HasMessage($"Access token successfully validated for user {user.Id}");
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
            this.timeProvider.GetUtcDateTimeNow().Subtract(tokenAge ?? new(24, 0, 0)));

        this.accessTokenEncoderMock
            .Setup(x => x.Decode(accessToken))
            .Returns(accessTokenPayload);
    }

    #endregion
}
