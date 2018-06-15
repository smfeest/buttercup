using System;
using System.Data.Common;
using System.Threading.Tasks;
using Buttercup.Models;
using Xunit;

namespace Buttercup.DataAccess
{
    [Collection("Database collection")]
    public class UserDataProviderTests
    {
        private static int sampleUserCount;

        private readonly DatabaseFixture databaseFixture;

        public UserDataProviderTests(DatabaseFixture databaseFixture) =>
            this.databaseFixture = databaseFixture;

        #region FindUserByEmail

        [Fact]
        public async Task FindUserByEmailReturnsUser() =>
            await this.databaseFixture.WithRollback(async connection =>
        {
            await InsertSampleUser(connection, CreateSampleUser(id: 4, email: "alpha@example.com"));

            var actual = await new UserDataProvider().FindUserByEmail(
                connection, "alpha@example.com");

            Assert.Equal(4, actual.Id);
            Assert.Equal("alpha@example.com", actual.Email);
        });

        [Fact]
        public async Task FindUserByEmailReturnsNullIfNoMatchFound() =>
            await this.databaseFixture.WithRollback(async connection =>
        {
            await InsertSampleUser(connection, CreateSampleUser(email: "alpha@example.com"));

            var actual = await new UserDataProvider().FindUserByEmail(
                connection, "beta@example.com");

            Assert.Null(actual);
        });

        #endregion

        #region ReadUser

        [Fact]
        public Task ReadUserReadsAllAttributes() =>
            this.databaseFixture.WithRollback(async connection =>
        {
            var expected = CreateSampleUser();

            await InsertSampleUser(connection, expected);

            var actual = await new UserDataProvider().FindUserByEmail(connection, expected.Email);

            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Email, actual.Email);
            Assert.Equal(expected.HashedPassword, actual.HashedPassword);
            Assert.Equal(expected.Created, actual.Created);
            Assert.Equal(DateTimeKind.Utc, actual.Created.Kind);
            Assert.Equal(expected.Modified, actual.Modified);
            Assert.Equal(DateTimeKind.Utc, actual.Modified.Kind);
            Assert.Equal(expected.Revision, actual.Revision);
        });

        #endregion

        private static User CreateSampleUser(long? id = null, string email = null)
        {
            var i = ++sampleUserCount;

            return new User
            {
                Id = id ?? i,
                Email = email ?? $"user-{i}@example.com",
                HashedPassword = $"user-{i}-password",
                Created = new DateTime(2001, 2, 3, 4, 5, 6),
                Modified = new DateTime(2002, 3, 4, 5, 6, 7),
                Revision = i + 1,
            };
        }

        private static async Task InsertSampleUser(DbConnection connection, User user)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"INSERT user(id, email, hashed_password, created, modified, revision)
                VALUES (@id, @email, @hashed_password, @created, @modified, @revision);";

                command.AddParameterWithValue("@id", user.Id);
                command.AddParameterWithValue("@email", user.Email);
                command.AddParameterWithValue("@hashed_password", user.HashedPassword);
                command.AddParameterWithValue("@created", user.Created);
                command.AddParameterWithValue("@modified", user.Modified);
                command.AddParameterWithValue("@revision", user.Revision);

                await command.ExecuteNonQueryAsync();
            }
        }
    }
}
