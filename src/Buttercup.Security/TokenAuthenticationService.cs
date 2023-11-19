using System.Net;
using System.Security.Cryptography;
using Buttercup.EntityModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Buttercup.Security;

internal sealed class TokenAuthenticationService(
    IAccessTokenEncoder accessTokenEncoder,
    IClock clock,
    IDbContextFactory<AppDbContext> dbContextFactory,
    ILogger<TokenAuthenticationService> logger)
    : ITokenAuthenticationService
{
    private readonly IAccessTokenEncoder accessTokenEncoder = accessTokenEncoder;
    private readonly IClock clock = clock;
    private readonly IDbContextFactory<AppDbContext> dbContextFactory = dbContextFactory;
    private readonly ILogger<TokenAuthenticationService> logger = logger;

    public async Task<string> IssueAccessToken(User user, IPAddress? ipAddress)
    {
        var token = this.accessTokenEncoder.Encode(
            new(user.Id, user.SecurityStamp, this.clock.UtcNow));

        using (var dbContext = this.dbContextFactory.CreateDbContext())
        {
            dbContext.SecurityEvents.Add(new()
            {
                Time = this.clock.UtcNow,
                Event = "access_token_issued",
                IpAddress = ipAddress,
                UserId = user.Id,
            });

            await dbContext.SaveChangesAsync();
        }

        LogMessages.TokenIssued(this.logger, user.Id, user.Email, null);

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
            LogMessages.ValidationFailedIncorrectEncoding(this.logger, exception);

            return null;
        }
        catch (CryptographicException exception)
        {
            LogMessages.ValidationFailedEncryptionError(this.logger, exception);

            return null;
        }

        if (this.clock.UtcNow.Subtract(payload.Issued) > new TimeSpan(24, 0, 0))
        {
            LogMessages.ValidationFailedExpired(this.logger, payload.UserId, null);

            return null;
        }

        using var dbContext = this.dbContextFactory.CreateDbContext();

        var user = await dbContext.Users.FindAsync(payload.UserId);

        if (user is null)
        {
            LogMessages.ValidationFailedUserDoesNotExist(this.logger, payload.UserId, null);

            return null;
        }

        if (!string.Equals(payload.SecurityStamp, user.SecurityStamp, StringComparison.Ordinal))
        {
            LogMessages.ValidationFailedStaleSecurityStamp(this.logger, payload.UserId, null);

            return null;
        }

        LogMessages.ValidationSuccessful(this.logger, payload.UserId, null);

        return user;
    }

    private static class LogMessages
    {
        public static readonly Action<ILogger, long, string, Exception?> TokenIssued =
            LoggerMessage.Define<long, string>(
                LogLevel.Information, 300, "Issued access token for user {UserId} ({Email})");

        public static readonly Action<ILogger, Exception?> ValidationFailedIncorrectEncoding =
            LoggerMessage.Define(
                LogLevel.Warning, 301, "Access token failed validation; not base64url encoded");

        public static readonly Action<ILogger, Exception?> ValidationFailedEncryptionError =
            LoggerMessage.Define(
                LogLevel.Warning,
                302,
                "Access token failed validation; malformed or encrypted with wrong key");

        public static readonly Action<ILogger, long, Exception?> ValidationFailedExpired =
            LoggerMessage.Define<long>(
                LogLevel.Information,
                303,
                "Access token failed validation for user {UserId}; expired");

        public static readonly Action<ILogger, long, Exception?> ValidationFailedUserDoesNotExist =
            LoggerMessage.Define<long>(
                LogLevel.Warning,
                304,
                "Access token failed validation for user {UserId}; user does not exist");

        public static readonly Action<ILogger, long, Exception?>
            ValidationFailedStaleSecurityStamp = LoggerMessage.Define<long>(
                LogLevel.Information,
                305,
                "Access token failed validation for user {UserId}; contains stale security stamp");

        public static readonly Action<ILogger, long, Exception?> ValidationSuccessful =
            LoggerMessage.Define<long>(
                LogLevel.Information, 306, "Access token successfully validated for user {UserId}");
    }
}
