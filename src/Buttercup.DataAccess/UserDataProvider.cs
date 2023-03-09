using Buttercup.Models;
using MySqlConnector;

namespace Buttercup.DataAccess;

internal sealed class UserDataProvider : IUserDataProvider
{
    private readonly IClock clock;

    public UserDataProvider(IClock clock) => this.clock = clock;

    public async Task<User?> FindUserByEmail(MySqlConnection connection, string email)
    {
        using var command = connection.CreateCommand();

        command.CommandText = "SELECT * FROM user WHERE email = @email";
        command.Parameters.AddWithValue("@email", email);

        using var reader = await command.ExecuteReaderAsync();

        return await reader.ReadAsync() ? ReadUser(reader) : null;
    }

    public async Task<User> GetUser(MySqlConnection connection, long id)
    {
        using var command = connection.CreateCommand();

        command.CommandText = "SELECT * FROM user WHERE id = @id";
        command.Parameters.AddWithValue("@id", id);

        using var reader = await command.ExecuteReaderAsync();

        return await reader.ReadAsync() ? ReadUser(reader) : throw UserNotFound(id);
    }

    public async Task<IList<User>> GetUsers(
        MySqlConnection connection, IReadOnlyCollection<long> ids)
    {
        if (ids.Count == 0)
        {
            return Array.Empty<User>();
        }

        using var command = connection.CreateCommand();

        command.CommandText =
            $"SELECT * FROM user WHERE id IN ({string.Join(',', ids)}) ORDER BY id";

        using var reader = await command.ExecuteReaderAsync();

        var users = new List<User>();

        while (await reader.ReadAsync())
        {
            users.Add(ReadUser(reader));
        }

        return users;
    }

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

    private static User ReadUser(MySqlDataReader reader) => new(
        reader.GetInt64("id"),
        reader.GetString("name"),
        reader.GetString("email"),
        reader.GetNullableString("hashed_password"),
        reader.GetNullableDateTime("password_created"),
        reader.GetString("security_stamp"),
        reader.GetString("time_zone"),
        reader.GetDateTime("created"),
        reader.GetDateTime("modified"),
        reader.GetInt32("revision"));

    private static NotFoundException UserNotFound(long userId) =>
        new($"User {userId} not found");
}
