using System.Data.Common;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Buttercup.DataAccess
{
    /// <summary>
    /// The default implementation of <see cref="IAuthenticationEventDataProvider" />.
    /// </summary>
    internal sealed class AuthenticationEventDataProvider : IAuthenticationEventDataProvider
    {
        private readonly IClock clock;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthenticationEventDataProvider" /> class.
        /// </summary>
        /// <param name="clock">
        /// The clock.
        /// </param>
        public AuthenticationEventDataProvider(IClock clock) => this.clock = clock;

        /// <inheritdoc />
        public async Task<long> LogEvent(
            DbConnection connection, string eventName, long? userId = null, string email = null)
        {
            using (var command = (MySqlCommand)connection.CreateCommand())
            {
                command.CommandText = @"INSERT authentication_event (time, event, user_id, email)
VALUES (@time, @event, @user_id, @email)";

                command.AddParameterWithValue("@time", this.clock.UtcNow);
                command.AddParameterWithStringValue("@event", eventName);
                command.AddParameterWithValue("@user_id", userId);
                command.AddParameterWithStringValue("@email", email);

                await command.ExecuteNonQueryAsync();

                return command.LastInsertedId;
            }
        }
    }
}
