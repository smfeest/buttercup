using Buttercup.Models;
using MySqlConnector;

namespace Buttercup.DataAccess;

/// <summary>
/// The default implementation of <see cref="IUserDataProvider" />.
/// </summary>
internal sealed class UserDataProvider : IUserDataProvider
{
    private readonly IClock clock;

    public UserDataProvider(IClock clock) => this.clock = clock;

    /// <inheritdoc />
    public async Task<User?> FindUserByEmail(MySqlConnection connection, string email)
    {
        using var command = connection.CreateCommand();

        command.CommandText = "SELECT * FROM user WHERE email = @email";
        command.Parameters.AddWithValue("@email", email);

        using var reader = await command.ExecuteReaderAsync();

        return await reader.ReadAsync() ? ReadUser(reader) : null;
    }

    /// <inheritdoc />
    public async Task<User> GetUser(MySqlConnection connection, long id)
    {
        using var command = connection.CreateCommand();

        command.CommandText = "SELECT * FROM user WHERE id = @id";
        command.Parameters.AddWithValue("@id", id);

        using var reader = await command.ExecuteReaderAsync();

        return await reader.ReadAsync() ? ReadUser(reader) : throw UserNotFound(id);
    }

    /// <inheritdoc />
    public async Task UpdatePassword(
        MySqlConnection connection, long userId, string hashedPassword, string securityStamp)
    {
        using var command = connection.CreateCommand();

        command.CommandText = @"UPDATE user
            SET hashed_password = @hashed_password,
                security_stamp = @security_stamp,
                password_created = @time,
                modified = @time,
                revision = revision + 1
            WHERE id = @id";
        command.Parameters.AddWithValue("@id", userId);
        command.Parameters.AddWithValue("@hashed_password", hashedPassword);
        command.Parameters.AddWithValue("@security_stamp", securityStamp);
        command.Parameters.AddWithValue("@time", this.clock.UtcNow);

        if (await command.ExecuteNonQueryAsync() == 0)
        {
            throw UserNotFound(userId);
        }
    }

    /// <inheritdoc />
    public async Task UpdatePreferences(MySqlConnection connection, long userId, string timeZone)
    {
        using var command = connection.CreateCommand();

        command.CommandText = @"UPDATE user
            SET time_zone = @time_zone,
                modified = @time,
                revision = revision + 1
            WHERE id = @id";
        command.Parameters.AddWithValue("@id", userId);
        command.Parameters.AddWithValue("@time_zone", timeZone);
        command.Parameters.AddWithValue("@time", this.clock.UtcNow);

        if (await command.ExecuteNonQueryAsync() == 0)
        {
            throw UserNotFound(userId);
        }
    }

    private static User ReadUser(MySqlDataReader reader) =>
        new()
        {
            Id = reader.GetInt64("id"),
            Name = reader.GetString("name"),
            Email = reader.GetString("email"),
            HashedPassword = reader.GetNullableString("hashed_password"),
            PasswordCreated = reader.GetNullableDateTime("password_created"),
            SecurityStamp = reader.GetString("security_stamp"),
            TimeZone = reader.GetString("time_zone"),
            Created = reader.GetDateTime("created"),
            Modified = reader.GetDateTime("modified"),
            Revision = reader.GetInt32("revision"),
        };

    private static NotFoundException UserNotFound(long userId) =>
        new($"User {userId} not found");
}
