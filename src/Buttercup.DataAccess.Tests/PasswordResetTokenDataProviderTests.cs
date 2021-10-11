using System;
using System.Threading.Tasks;
using Xunit;

namespace Buttercup.DataAccess
{
    [Collection("Database collection")]
    public class PasswordResetTokenDataProviderTests
    {
        private readonly DatabaseFixture databaseFixture;

        public PasswordResetTokenDataProviderTests(DatabaseFixture databaseFixture) =>
            this.databaseFixture = databaseFixture;

        #region DeleteExpiredTokens

        [Fact]
        public Task DeleteExpiredTokensDeletesExpiredTokens() =>
            TestDatabase.WithRollback(async connection =>
        {
            var passwordResetTokenDataProvider = new PasswordResetTokenDataProvider();

            await SampleUsers.InsertSampleUser(connection, SampleUsers.CreateSampleUser(id: 3));

            await passwordResetTokenDataProvider.InsertToken(
                connection, 3, "token-a", new(2000, 1, 2, 11, 59, 59));

            await passwordResetTokenDataProvider.InsertToken(
                connection, 3, "token-b", new(2000, 1, 2, 12, 00, 00));

            await passwordResetTokenDataProvider.InsertToken(
                connection, 3, "token-c", new(2000, 1, 2, 12, 00, 01));

            await passwordResetTokenDataProvider.DeleteExpiredTokens(
                connection, new(2000, 1, 2, 12, 00, 00));

            string? survivingTokens;

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT GROUP_CONCAT(token) FROM password_reset_token";
                survivingTokens = (string?)await command.ExecuteScalarAsync();
            }

            Assert.Equal("token-b,token-c", survivingTokens);
        });

        #endregion

        #region DeleteTokensForUser

        [Fact]
        public Task DeleteTokensForUserDeletesTokensBelongingToUser() =>
            TestDatabase.WithRollback(async connection =>
        {
            var passwordResetTokenDataProvider = new PasswordResetTokenDataProvider();

            await SampleUsers.InsertSampleUser(connection, SampleUsers.CreateSampleUser(id: 7));
            await SampleUsers.InsertSampleUser(connection, SampleUsers.CreateSampleUser(id: 11));

            var time = DateTime.UtcNow;

            await passwordResetTokenDataProvider.InsertToken(connection, 7, "token-a", time);
            await passwordResetTokenDataProvider.InsertToken(connection, 11, "token-b", time);
            await passwordResetTokenDataProvider.InsertToken(connection, 7, "token-c", time);

            await passwordResetTokenDataProvider.DeleteTokensForUser(connection, 7);

            string? survivingTokens;

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT GROUP_CONCAT(token) FROM password_reset_token";
                survivingTokens = (string?)await command.ExecuteScalarAsync();
            }

            Assert.Equal("token-b", survivingTokens);
        });

        #endregion

        #region GetUserIdForToken

        [Fact]
        public async Task GetUserIdForTokenReturnsUserIdWhenTokenExists() =>
            await TestDatabase.WithRollback(async connection =>
        {
            var passwordResetTokenDataProvider = new PasswordResetTokenDataProvider();

            await SampleUsers.InsertSampleUser(connection, SampleUsers.CreateSampleUser(id: 5));
            await passwordResetTokenDataProvider.InsertToken(
                connection, 5, "sample-token", DateTime.UtcNow);

            var actual = await passwordResetTokenDataProvider.GetUserIdForToken(
                connection, "sample-token");

            Assert.Equal(5, actual);
        });

        [Fact]
        public async Task GetUserIdForTokenReturnsNullIfNoMatchFound() =>
            await TestDatabase.WithRollback(async connection =>
        {
            var actual = await new PasswordResetTokenDataProvider().GetUserIdForToken(
                connection, "sample-token");

            Assert.Null(actual);
        });

        #endregion

        #region InsertToken

        [Fact]
        public Task InsertTokenInsertsToken() =>
            TestDatabase.WithRollback(async connection =>
        {
            await SampleUsers.InsertSampleUser(connection, SampleUsers.CreateSampleUser(id: 6));

            var time = new DateTime(2000, 1, 2, 3, 4, 5);

            await new PasswordResetTokenDataProvider().InsertToken(
                connection, 6, "sample-token", time);

            using var command = connection.CreateCommand();

            command.CommandText = "SELECT * FROM password_reset_token WHERE token = 'sample-token'";

            using var reader = await command.ExecuteReaderAsync();

            await reader.ReadAsync();

            Assert.Equal(6, reader.GetInt64("user_id"));
            Assert.Equal(time, reader.GetDateTime("created", DateTimeKind.Utc));
        });

        #endregion
    }
}
