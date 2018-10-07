using System;
using System.Data.Common;
using System.Threading.Tasks;
using Buttercup.Models;

namespace Buttercup.DataAccess
{
    /// <summary>
    /// The default implementation of <see cref="IUserDataProvider" />.
    /// </summary>
    internal sealed class UserDataProvider : IUserDataProvider
    {
        private readonly IClock clock;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserDataProvider" /> class.
        /// </summary>
        /// <param name="clock">
        /// The clock.
        /// </param>
        public UserDataProvider(IClock clock) => this.clock = clock;

        /// <inheritdoc />
        public async Task<User> FindUserByEmail(DbConnection connection, string email)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM user WHERE email = @email";
                command.AddParameterWithValue("@email", email);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (!await reader.ReadAsync())
                    {
                        return null;
                    }

                    return ReadUser(reader);
                }
            }
        }

        /// <inheritdoc />
        public async Task<User> GetUser(DbConnection connection, long id)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM user WHERE id = @id";
                command.AddParameterWithValue("@id", id);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (!await reader.ReadAsync())
                    {
                        throw new NotFoundException($"User {id} not found");
                    }

                    return ReadUser(reader);
                }
            }
        }

        /// <inheritdoc />
        public async Task UpdatePassword(
            DbConnection connection, long userId, string hashedPassword, string securityStamp)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"UPDATE user
                    SET hashed_password = @hashed_password,
                        security_stamp = @security_stamp,
                        modified = @modified,
                        revision = revision + 1
                    WHERE id = @id";
                command.AddParameterWithValue("@id", userId);
                command.AddParameterWithValue("@hashed_password", hashedPassword);
                command.AddParameterWithValue("@security_stamp", securityStamp);
                command.AddParameterWithValue("@modified", this.clock.UtcNow);

                if (await command.ExecuteNonQueryAsync() == 0)
                {
                    throw new NotFoundException($"User {userId} not found");
                }
            }
        }

        private static User ReadUser(DbDataReader reader) =>
            new User
            {
                Id = reader.GetInt64("id"),
                Email = reader.GetString("email"),
                HashedPassword = reader.GetString("hashed_password"),
                SecurityStamp = reader.GetString("security_stamp"),
                Created = reader.GetDateTime("created", DateTimeKind.Utc),
                Modified = reader.GetDateTime("modified", DateTimeKind.Utc),
                Revision = reader.GetInt32("revision"),
            };
    }
}
