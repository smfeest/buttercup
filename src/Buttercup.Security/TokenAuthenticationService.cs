using System.Net;
using System.Security.Cryptography;
using Buttercup.EntityModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Buttercup.Security;

internal sealed partial class TokenAuthenticationService(
    IAccessTokenEncoder accessTokenEncoder,
    IDbContextFactory<AppDbContext> dbContextFactory,
    ILogger<TokenAuthenticationService> logger,
    TimeProvider timeProvider)
    : ITokenAuthenticationService
{
    private readonly IAccessTokenEncoder accessTokenEncoder = accessTokenEncoder;
    private readonly IDbContextFactory<AppDbContext> dbContextFactory = dbContextFactory;
    private readonly ILogger<TokenAuthenticationService> logger = logger;
    private readonly TimeProvider timeProvider = timeProvider;

    public async Task<string> IssueAccessToken(User user, IPAddress? ipAddress)
    {
        var timestamp = this.timeProvider.GetUtcDateTimeNow();
        var token = this.accessTokenEncoder.Encode(new(user.Id, user.SecurityStamp, timestamp));

        using var dbContext = this.dbContextFactory.CreateDbContext();

        dbContext.UserAuditEntries.Add(new()
        {
            Time = timestamp,
            Operation = UserAuditOperation.CreateAccessToken,
            TargetId = user.Id,
            ActorId = user.Id,
            IpAddress = ipAddress,
        });

        await dbContext.SaveChangesAsync();

        this.LogTokenIssued(user.Id, user.Email);

        return token;
    }

    public async Task<User?> ValidateAccessToken(string accessToken)
    {
        AccessTokenPayload payload;

        try
        {
            payload = this.accessTokenEncoder.Decode(accessToken);
        }
        catch (FormatException exception)
        {
            this.LogIncorrectEncoding(exception);
            return null;
        }
        catch (CryptographicException exception)
        {
            this.LogEncryptionError(exception);
            return null;
        }

        if (this.timeProvider.GetUtcDateTimeNow().Subtract(payload.Issued) > new TimeSpan(24, 0, 0))
        {
            this.LogTokenExpired(payload.UserId);
            return null;
        }

        using var dbContext = this.dbContextFactory.CreateDbContext();

        var user = await dbContext.Users.FindAsync(payload.UserId);

        if (user is null)
        {
            this.LogUserDoesNotExist(payload.UserId);
            return null;
        }

        if (!string.Equals(payload.SecurityStamp, user.SecurityStamp, StringComparison.Ordinal))
        {
            this.LogStaleSecurityStamp(payload.UserId);

            return null;
        }

        this.LogTokenValidated(payload.UserId);

        return user;
    }

    [LoggerMessage(
        EventId = 1,
        EventName = "EncryptionError",
        Level = LogLevel.Warning,
        Message = "Access token failed validation; malformed or encrypted with wrong key")]
    private partial void LogEncryptionError(Exception exception);

    [LoggerMessage(
        EventId = 2,
        EventName = "IncorrectEncoding",
        Level = LogLevel.Warning,
        Message = "Access token failed validation; not base64url encoded")]
    private partial void LogIncorrectEncoding(Exception exception);

    [LoggerMessage(
        EventId = 3,
        EventName = "StaleSecurityStamp",
        Level = LogLevel.Information,
        Message = "Access token failed validation for user {UserId}; contains stale security stamp")]
    private partial void LogStaleSecurityStamp(long userId);

    [LoggerMessage(
        EventId = 4,
        EventName = "TokenExpired",
        Level = LogLevel.Information,
        Message = "Access token failed validation for user {UserId}; expired")]
    private partial void LogTokenExpired(long userId);

    [LoggerMessage(
        EventId = 5,
        EventName = "TokenIssued",
        Level = LogLevel.Information,
        Message = "Issued access token for user {UserId} ({Email})")]
    private partial void LogTokenIssued(long userId, string email);

    [LoggerMessage(
        EventId = 6,
        EventName = "TokenValidated",
        Level = LogLevel.Information,
        Message = "Access token successfully validated for user {UserId}")]
    private partial void LogTokenValidated(long userId);

    [LoggerMessage(
        EventId = 7,
        EventName = "UserDoesNotExist",
        Level = LogLevel.Warning,
        Message = "Access token failed validation for user {UserId}; user does not exist")]
    private partial void LogUserDoesNotExist(long userId);
}
