using System.Net;
using System.Security.Cryptography;
using Buttercup.EntityModel;
using Buttercup.TestUtils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Buttercup.Security;

[Collection(nameof(DatabaseCollection))]
public sealed class TokenAuthenticationServiceTests : IAsyncLifetime
{
    private readonly DatabaseCollectionFixture databaseFixture;
    private readonly ModelFactory modelFactory = new();

    private readonly Mock<IAccessTokenEncoder> accessTokenEncoderMock = new();
    private readonly StoppedClock clock;
    private readonly ListLogger<TokenAuthenticationService> logger = new();

    private readonly TokenAuthenticationService tokenAuthenticationService;

    public TokenAuthenticationServiceTests(DatabaseCollectionFixture databaseFixture)
    {
        this.databaseFixture = databaseFixture;
        this.clock = new() { UtcNow = this.modelFactory.NextDateTime() };

        this.tokenAuthenticationService = new(
            this.accessTokenEncoderMock.Object, this.clock, this.databaseFixture, this.logger);
    }

    public Task InitializeAsync() => this.databaseFixture.ClearDatabase();

    public Task DisposeAsync() => Task.CompletedTask;

    #region IssueAccessToken

    [Fact]
    public async Task IssueAccessToken()
    {
        var accessToken = this.modelFactory.NextString("access-token");
        var ipAddress = new IPAddress(this.modelFactory.NextInt());
        var user = this.modelFactory.BuildUser();

        using (var dbContext = this.databaseFixture.CreateDbContext())
        {
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
        }

        this.accessTokenEncoderMock
            .Setup(x => x.Encode(new(user.Id, user.SecurityStamp, this.clock.UtcNow)))
            .Returns(accessToken);

        var returnedToken = await this.tokenAuthenticationService.IssueAccessToken(user, ipAddress);

        using (var dbContext = this.databaseFixture.CreateDbContext())
        {
            // Inserts security event
            Assert.True(
                await dbContext.SecurityEvents.AnyAsync(
                    securityEvent =>
                        securityEvent.Time == this.clock.UtcNow &&
                        securityEvent.Event == "access_token_issued" &&
                        securityEvent.IpAddress == ipAddress &&
                        securityEvent.UserId == user.Id));

            // Logs token issued message
            LogAssert.HasEntry(
                this.logger,
                LogLevel.Information,
                300,
                $"Issued access token for user {user.Id} ({user.Email})");

            // Returns token
            Assert.Equal(accessToken, returnedToken);
        }
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
        LogAssert.HasEntry(
            this.logger,
            LogLevel.Warning,
            301,
            "Access token failed validation; not base64url encoded",
            exception);
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
        LogAssert.HasEntry(
            this.logger,
            LogLevel.Warning,
            302,
            "Access token failed validation; malformed or encrypted with wrong key",
            exception);
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
        LogAssert.HasEntry(
            this.logger,
            LogLevel.Information,
            303,
            $"Access token failed validation for user {user.Id}; expired");
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
        LogAssert.HasEntry(
            this.logger,
            LogLevel.Warning,
            304,
            $"Access token failed validation for user {user.Id}; user does not exist");
    }

    [Fact]
    public async Task ValidateAccessToken_SecurityStampHasChanged()
    {
        var accessToken = this.modelFactory.NextString("access-token");
        var user = this.modelFactory.BuildUser();

        using (var dbContext = this.databaseFixture.CreateDbContext())
        {
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
        }

        this.SetupDecodeSuccess(accessToken, user, securityStamp: "stale-security-stamp");

        // Returns null
        Assert.Null(await this.tokenAuthenticationService.ValidateAccessToken(accessToken));

        // Logs stale security stamp message
        LogAssert.HasEntry(
            this.logger,
            LogLevel.Information,
            305,
            $"Access token failed validation for user {user.Id}; contains stale security stamp");
    }

    [Fact]
    public async Task ValidateAccessToken_Success()
    {
        var accessToken = this.modelFactory.NextString("access-token");
        var user = this.modelFactory.BuildUser();

        using (var dbContext = this.databaseFixture.CreateDbContext())
        {
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
        }

        this.SetupDecodeSuccess(accessToken, user);

        // Returns user
        Assert.Equal(user, await this.tokenAuthenticationService.ValidateAccessToken(accessToken));

        // Logs successfully validated message
        LogAssert.HasEntry(
            this.logger,
            LogLevel.Information,
            306,
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

    #endregion
}