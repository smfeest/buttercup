using System;
using System.Threading.Tasks;
using Moq;
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
            this.databaseFixture.WithRollback(async connection =>
        {
            var context = new Context();

            await SampleUsers.InsertSampleUser(connection, SampleUsers.CreateSampleUser(id: 3));

            context.SetupUtcNow(new DateTime(2000, 1, 2, 11, 59, 59));
            await context.PasswordResetTokenDataProvider.InsertToken(connection, 3, "token-a");

            context.SetupUtcNow(new DateTime(2000, 1, 2, 12, 00, 00));
            await context.PasswordResetTokenDataProvider.InsertToken(connection, 3, "token-b");

            context.SetupUtcNow(new DateTime(2000, 1, 2, 12, 00, 01));
            await context.PasswordResetTokenDataProvider.InsertToken(connection, 3, "token-c");

            context.SetupUtcNow(new DateTime(2000, 1, 3, 12, 00, 00));
            await context.PasswordResetTokenDataProvider.DeleteExpiredTokens(connection);

            string survivingTokens;

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT GROUP_CONCAT(token) FROM password_reset_token";
                survivingTokens = (string)await command.ExecuteScalarAsync();
            }

            Assert.Equal("token-b,token-c", survivingTokens);
        });

        #endregion

        #region GetUserIdForToken

        [Fact]
        public async Task GetUserIdForTokenReturnsUserIdWhenTokenExists() =>
            await this.databaseFixture.WithRollback(async connection =>
        {
            var context = new Context();

            await SampleUsers.InsertSampleUser(connection, SampleUsers.CreateSampleUser(id: 5));
            await context.PasswordResetTokenDataProvider.InsertToken(connection, 5, "sample-token");

            var actual = await context.PasswordResetTokenDataProvider.GetUserIdForToken(
                connection, "sample-token");

            Assert.Equal(5, actual);
        });

        [Fact]
        public async Task GetUserIdForTokenReturnsNullIfNoMatchFound() =>
            await this.databaseFixture.WithRollback(async connection =>
        {
            var actual = await new Context().PasswordResetTokenDataProvider.GetUserIdForToken(
                connection, "sample-token");

            Assert.Null(actual);
        });

        #endregion

        #region InsertToken

        [Fact]
        public Task InsertTokenInsertsToken() =>
            this.databaseFixture.WithRollback(async connection =>
        {
            var context = new Context();

            await SampleUsers.InsertSampleUser(connection, SampleUsers.CreateSampleUser(id: 6));

            var utcNow = new DateTime(2000, 1, 2, 3, 4, 5);
            context.SetupUtcNow(utcNow);

            await context.PasswordResetTokenDataProvider.InsertToken(connection, 6, "sample-token");

            using (var command = connection.CreateCommand())
            {
                command.CommandText =
                    "SELECT * FROM password_reset_token WHERE token = 'sample-token'";

                using (var reader = await command.ExecuteReaderAsync())
                {
                    await reader.ReadAsync();

                    Assert.Equal(6, reader.GetInt64("user_id"));
                    Assert.Equal(utcNow, reader.GetDateTime("created", DateTimeKind.Utc));
                }
            }
        });

        #endregion

        private class Context
        {
            public Context() =>
                this.PasswordResetTokenDataProvider = new PasswordResetTokenDataProvider(
                    this.MockClock.Object);

            public PasswordResetTokenDataProvider PasswordResetTokenDataProvider { get; }

            public Mock<IClock> MockClock { get; } = new Mock<IClock>();

            public void SetupUtcNow(DateTime utcNow) =>
                this.MockClock.SetupGet(x => x.UtcNow).Returns(utcNow);
        }
    }
}
