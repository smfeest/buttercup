using Buttercup.DataAccess;
using Buttercup.Models;

namespace Buttercup.Web.Authentication;

public class TokenAuthenticationService : ITokenAuthenticationService
{
    private readonly IAccessTokenEncoder accessTokenEncoder;
    private readonly IAuthenticationEventDataProvider authenticationEventDataProvider;
    private readonly IClock clock;
    private readonly ILogger<TokenAuthenticationService> logger;
    private readonly IMySqlConnectionSource mySqlConnectionSource;

    public TokenAuthenticationService(
        IAccessTokenEncoder accessTokenEncoder,
        IAuthenticationEventDataProvider authenticationEventDataProvider,
        IClock clock,
        ILogger<TokenAuthenticationService> logger,
        IMySqlConnectionSource mySqlConnectionSource)
    {
        this.accessTokenEncoder = accessTokenEncoder;
        this.authenticationEventDataProvider = authenticationEventDataProvider;
        this.clock = clock;
        this.logger = logger;
        this.mySqlConnectionSource = mySqlConnectionSource;
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

    private static class LogMessages
    {
        public static readonly Action<ILogger, long, string, Exception?> TokenIssued =
            LoggerMessage.Define<long, string>(
                LogLevel.Information, 300, "Issued access token for user {UserId} ({Email})");
    }
}
