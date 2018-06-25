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

        #region InsertToken

        [Fact]
        public Task InsertTokenInsertsToken() =>
            this.databaseFixture.WithRollback(async connection =>
        {
            var context = new Context();

            await SampleUsers.InsertSampleUser(connection, SampleUsers.CreateSampleUser(id: 6));

            var utcNow = new DateTime(2000, 1, 2, 3, 4, 5);
            context.MockClock.SetupGet(x => x.UtcNow).Returns(utcNow);

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
        }
    }
}
