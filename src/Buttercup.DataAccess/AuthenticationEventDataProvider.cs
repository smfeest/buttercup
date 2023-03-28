using MySqlConnector;

namespace Buttercup.DataAccess;

internal sealed class AuthenticationEventDataProvider : IAuthenticationEventDataProvider
{
    private readonly IClock clock;

    public AuthenticationEventDataProvider(IClock clock) => this.clock = clock;

    public async Task<long> LogEvent(
        MySqlConnection connection, string eventName, long? userId = null, string? email = null)
    {
        using var command = connection.CreateCommand();

        command.CommandText = @"INSERT authentication_events (time, event, user_id, email)
            VALUES (@time, @event, @user_id, @email)";

        command.Parameters.AddWithValue("@time", this.clock.UtcNow);
        command.Parameters.AddWithStringValue("@event", eventName);
        command.Parameters.AddWithValue("@user_id", userId);
        command.Parameters.AddWithStringValue("@email", email);

        await command.ExecuteNonQueryAsync();

        return command.LastInsertedId;
    }
}
