using MySqlConnector;

namespace Buttercup.DataAccess;

/// <summary>
/// The default implementation of <see cref="IAuthenticationEventDataProvider" />.
/// </summary>
internal sealed class AuthenticationEventDataProvider : IAuthenticationEventDataProvider
{
    /// <inheritdoc />
    public async Task<long> LogEvent(
        MySqlConnection connection,
        DateTime time,
        string eventName,
        long? userId = null,
        string? email = null)
    {
        using var command = connection.CreateCommand();

        command.CommandText = @"INSERT authentication_event (time, event, user_id, email)
            VALUES (@time, @event, @user_id, @email)";

        command.Parameters.AddWithValue("@time", time);
        command.Parameters.AddWithStringValue("@event", eventName);
        command.Parameters.AddWithValue("@user_id", userId);
        command.Parameters.AddWithStringValue("@email", email);

        await command.ExecuteNonQueryAsync();

        return command.LastInsertedId;
    }
}
