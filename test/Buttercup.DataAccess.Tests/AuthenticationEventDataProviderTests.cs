using System;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace Buttercup.DataAccess
{
    [Collection("Database collection")]
    public class AuthenticationEventDataProviderTests
    {
        private readonly DatabaseFixture databaseFixture;

        public AuthenticationEventDataProviderTests(DatabaseFixture databaseFixture) =>
            this.databaseFixture = databaseFixture;

        #region LogEvent

        [Fact]
        public Task LogEventInsertsEvent() => this.databaseFixture.WithRollback(async connection =>
        {
            await SampleUsers.InsertSampleUser(connection, SampleUsers.CreateSampleUser(id: 8));

            var id = await new AuthenticationEventDataProvider().LogEvent(
                connection,
                new DateTime(2000, 1, 2, 3, 4, 5),
                "sample-event",
                8,
                "sample@example.com");

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM authentication_event WHERE id = @id";
                command.AddParameterWithValue("@id", id);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    await reader.ReadAsync();

                    Assert.Equal(
                        new DateTime(2000, 1, 2, 3, 4, 5),
                        reader.GetDateTime("time", DateTimeKind.Utc));
                    Assert.Equal("sample-event", reader.GetString("event"));
                    Assert.Equal(8, reader.GetInt64("user_id"));
                    Assert.Equal("sample@example.com", reader.GetString("email"));
                }
            }
        });

        [Fact]
        public Task LogEventAcceptsNullUserIdAndEmail() =>
            this.databaseFixture.WithRollback(async connection =>
        {
            var id = await new AuthenticationEventDataProvider().LogEvent(
                connection, DateTime.UtcNow, "sample-event");

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM authentication_event WHERE id = @id";
                command.AddParameterWithValue("@id", id);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    await reader.ReadAsync();

                    Assert.True(reader.IsDBNull(reader.GetOrdinal("user_id")));
                    Assert.True(reader.IsDBNull(reader.GetOrdinal("email")));
                }
            }
        });

        #endregion
    }
}
