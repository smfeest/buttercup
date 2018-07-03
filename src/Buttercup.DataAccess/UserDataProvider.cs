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

        private static User ReadUser(DbDataReader reader) =>
            new User
            {
                Id = reader.GetInt64("id"),
                Email = reader.GetString("email"),
                HashedPassword = reader.GetString("hashed_password"),
                Created = reader.GetDateTime("created", DateTimeKind.Utc),
                Modified = reader.GetDateTime("modified", DateTimeKind.Utc),
                Revision = reader.GetInt32("revision"),
            };
    }
}
