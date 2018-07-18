using System.Data.Common;
using System.Threading.Tasks;

namespace Buttercup.DataAccess
{
    /// <summary>
    /// The default implementation of <see cref="IPasswordResetTokenDataProvider" />.
    /// </summary>
    internal sealed class PasswordResetTokenDataProvider : IPasswordResetTokenDataProvider
    {
        private readonly IClock clock;

        /// <summary>
        /// Initializes a new instance of the <see cref="PasswordResetTokenDataProvider" /> class.
        /// </summary>
        /// <param name="clock">
        /// The clock.
        /// </param>
        public PasswordResetTokenDataProvider(IClock clock) => this.clock = clock;

        /// <inheritdoc />
        public async Task DeleteExpiredTokens(DbConnection connection)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"DELETE FROM password_reset_token WHERE created < @cut_off";
                command.AddParameterWithValue("@cut_off", this.clock.UtcNow.AddDays(-1));

                await command.ExecuteNonQueryAsync();
            }
        }

        /// <inheritdoc />
        public async Task DeleteTokensForUser(DbConnection connection, long userId)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"DELETE FROM password_reset_token WHERE user_id = @user_id";
                command.AddParameterWithValue("@user_id", userId);

                await command.ExecuteNonQueryAsync();
            }
        }

        /// <inheritdoc />
        public async Task<long?> GetUserIdForToken(
            DbConnection connection, string token)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText =
                    "SELECT user_id FROM password_reset_token WHERE token = @token";
                command.AddParameterWithValue("@token", token);

                return await command.ExecuteScalarAsync<uint?>();
            }
        }

        /// <inheritdoc />
        public async Task InsertToken(DbConnection connection, long userId, string token)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"INSERT password_reset_token(token, user_id, created)
VALUES(@token, @user_id, @created)";
                command.AddParameterWithValue("@token", token);
                command.AddParameterWithValue("@user_id", userId);
                command.AddParameterWithValue("@created", this.clock.UtcNow);

                await command.ExecuteNonQueryAsync();
            }
        }
    }
}
