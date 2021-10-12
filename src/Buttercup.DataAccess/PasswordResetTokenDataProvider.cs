using System;
using System.Threading.Tasks;
using MySqlConnector;

namespace Buttercup.DataAccess
{
    /// <summary>
    /// The default implementation of <see cref="IPasswordResetTokenDataProvider" />.
    /// </summary>
    internal sealed class PasswordResetTokenDataProvider : IPasswordResetTokenDataProvider
    {
        /// <inheritdoc />
        public async Task DeleteExpiredTokens(MySqlConnection connection, DateTime cutOff)
        {
            using var command = connection.CreateCommand();

            command.CommandText = @"DELETE FROM password_reset_token WHERE created < @cut_off";
            command.Parameters.AddWithValue("@cut_off", cutOff);

            await command.ExecuteNonQueryAsync();
        }

        /// <inheritdoc />
        public async Task DeleteTokensForUser(MySqlConnection connection, long userId)
        {
            using var command = connection.CreateCommand();

            command.CommandText = @"DELETE FROM password_reset_token WHERE user_id = @user_id";
            command.Parameters.AddWithValue("@user_id", userId);

            await command.ExecuteNonQueryAsync();
        }

        /// <inheritdoc />
        public async Task<long?> GetUserIdForToken(MySqlConnection connection, string token)
        {
            using var command = connection.CreateCommand();

            command.CommandText = "SELECT user_id FROM password_reset_token WHERE token = @token";
            command.Parameters.AddWithValue("@token", token);

            return await command.ExecuteScalarAsync<uint?>();
        }

        /// <inheritdoc />
        public async Task InsertToken(
            MySqlConnection connection, long userId, string token, DateTime created)
        {
            using var command = connection.CreateCommand();

            command.CommandText = @"INSERT password_reset_token(token, user_id, created)
                VALUES(@token, @user_id, @created)";
            command.Parameters.AddWithValue("@token", token);
            command.Parameters.AddWithValue("@user_id", userId);
            command.Parameters.AddWithValue("@created", created);

            await command.ExecuteNonQueryAsync();
        }
    }
}
