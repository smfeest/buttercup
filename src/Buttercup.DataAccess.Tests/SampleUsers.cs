using Buttercup.Models;
using MySqlConnector;

namespace Buttercup.DataAccess
{
    public static class SampleUsers
    {
        private static int sampleUserCount;

        public static User CreateSampleUser(
            bool includeOptionalAttributes = false,
            long? id = null,
            string? email = null,
            int? revision = null)
        {
            var i = ++sampleUserCount;

            var user = new User
            {
                Id = id ?? i,
                Name = $"user-{i}-name",
                Email = email ?? $"user-{i}@example.com",
                SecurityStamp = "secstamp",
                TimeZone = $"user-{i}-time-zone",
                Created = new DateTime(2001, 2, 3, 4, 5, 6).AddSeconds(i),
                Modified = new DateTime(2002, 3, 4, 5, 6, 7).AddSeconds(i),
                Revision = revision ?? (i + 1),
            };

            if (includeOptionalAttributes)
            {
                user.HashedPassword = $"user-{i}-password";
                user.PasswordCreated = new DateTime(2000, 1, 2, 3, 4, 5).AddSeconds(i);
            }

            return user;
        }

        public static async Task InsertSampleUser(MySqlConnection connection, User user)
        {
            using var command = connection.CreateCommand();

            command.CommandText = @"INSERT user(id, name, email, hashed_password, password_created, security_stamp, time_zone, created, modified, revision)
                VALUES (@id, @name, @email, @hashed_password, @password_created, @security_stamp, @time_zone, @created, @modified, @revision);";

            command.Parameters.AddWithValue("@id", user.Id);
            command.Parameters.AddWithValue("@name", user.Name);
            command.Parameters.AddWithValue("@email", user.Email);
            command.Parameters.AddWithValue("@hashed_password", user.HashedPassword);
            command.Parameters.AddWithValue("@password_created", user.PasswordCreated);
            command.Parameters.AddWithValue("@security_stamp", user.SecurityStamp);
            command.Parameters.AddWithValue("@time_zone", user.TimeZone);
            command.Parameters.AddWithValue("@created", user.Created);
            command.Parameters.AddWithValue("@modified", user.Modified);
            command.Parameters.AddWithValue("@revision", user.Revision);

            await command.ExecuteNonQueryAsync();
        }
    }
}
