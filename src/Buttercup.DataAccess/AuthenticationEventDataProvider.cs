using System;
using System.Threading.Tasks;
using MySqlConnector;

namespace Buttercup.DataAccess
{
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

            command.AddParameterWithValue("@time", time);
            command.AddParameterWithStringValue("@event", eventName);
            command.AddParameterWithValue("@user_id", userId);
            command.AddParameterWithStringValue("@email", email);

            await command.ExecuteNonQueryAsync();

            return command.LastInsertedId;
        }
    }
}
