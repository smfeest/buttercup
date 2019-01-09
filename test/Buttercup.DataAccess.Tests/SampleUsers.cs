using System;
using System.Data.Common;
using System.Threading.Tasks;
using Buttercup.Models;

namespace Buttercup.DataAccess
{
    public static class SampleUsers
    {
        private static int sampleUserCount;

        public static User CreateSampleUser(
            long? id = null, string email = null, int? revision = null)
        {
            var i = ++sampleUserCount;

            return new User
            {
                Id = id ?? i,
                Name = $"user-{i}-name",
                Email = email ?? $"user-{i}@example.com",
                HashedPassword = $"user-{i}-password",
                PasswordCreated = new DateTime(2000, 1, 2, 3, 4, 5),
                SecurityStamp = "secstamp",
                TimeZone = $"user-{i}-time-zone",
                Created = new DateTime(2001, 2, 3, 4, 5, 6),
                Modified = new DateTime(2002, 3, 4, 5, 6, 7),
                Revision = revision ?? (i + 1),
            };
        }

        public static async Task InsertSampleUser(DbConnection connection, User user)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"INSERT user(id, name, email, hashed_password, password_created, security_stamp, time_zone, created, modified, revision)
                VALUES (@id, @name, @email, @hashed_password, @password_created, @security_stamp, @time_zone, @created, @modified, @revision);";

                command.AddParameterWithValue("@id", user.Id);
                command.AddParameterWithValue("@name", user.Name);
                command.AddParameterWithValue("@email", user.Email);
                command.AddParameterWithValue("@hashed_password", user.HashedPassword);
                command.AddParameterWithValue("@password_created", user.PasswordCreated);
                command.AddParameterWithValue("@security_stamp", user.SecurityStamp);
                command.AddParameterWithValue("@time_zone", user.TimeZone);
                command.AddParameterWithValue("@created", user.Created);
                command.AddParameterWithValue("@modified", user.Modified);
                command.AddParameterWithValue("@revision", user.Revision);

                await command.ExecuteNonQueryAsync();
            }
        }
    }
}
