using System.Security.Cryptography;
using Buttercup.DataAccess;
using Buttercup.EntityModel;

namespace Buttercup.Web.Authentication;

public class TokenAuthenticationService : ITokenAuthenticationService
{
    private readonly IAccessTokenEncoder accessTokenEncoder;
    private readonly IAuthenticationEventDataProvider authenticationEventDataProvider;
    private readonly IClock clock;
    private readonly ILogger<TokenAuthenticationService> logger;
    private readonly IMySqlConnectionSource mySqlConnectionSource;
    private readonly IUserDataProvider userDataProvider;

    public TokenAuthenticationService(
        IAccessTokenEncoder accessTokenEncoder,
        IAuthenticationEventDataProvider authenticationEventDataProvider,
        IClock clock,
        ILogger<TokenAuthenticationService> logger,
        IMySqlConnectionSource mySqlConnectionSource,
        IUserDataProvider userDataProvider)
    {
        this.accessTokenEncoder = accessTokenEncoder;
        this.authenticationEventDataProvider = authenticationEventDataProvider;
        this.clock = clock;
        this.logger = logger;
        this.mySqlConnectionSource = mySqlConnectionSource;
        this.userDataProvider = userDataProvider;
    }

    public async Task<string> IssueAccessToken(User user)
    {
        var token = this.accessTokenEncoder.Encode(
            new(user.Id, user.SecurityStamp, this.clock.UtcNow));

        LogMessages.TokenIssued(this.logger, user.Id, user.Email, null);

        using var connection = await this.mySqlConnectionSource.OpenConnection();

        await this.authenticationEventDataProvider.LogEvent(
            connection, "access_token_issued", user.Id);

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

        User user;

        try
        {
            using var connection = await this.mySqlConnectionSource.OpenConnection();

            user = await this.userDataProvider.GetUser(connection, payload.UserId);
        }
        catch (NotFoundException)
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
