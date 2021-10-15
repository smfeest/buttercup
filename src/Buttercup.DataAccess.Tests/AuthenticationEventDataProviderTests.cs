using System;
using System.Threading.Tasks;
using Xunit;

namespace Buttercup.DataAccess
{
    [Collection("Database collection")]
    public class AuthenticationEventDataProviderTests
    {
        #region LogEvent

        [Fact]
        public async Task LogEventInsertsEvent()
        {
            using var connection = await TestDatabase.OpenConnectionWithRollback();

            await SampleUsers.InsertSampleUser(connection, SampleUsers.CreateSampleUser(id: 8));

            var id = await new AuthenticationEventDataProvider().LogEvent(
                connection,
                new(2000, 1, 2, 3, 4, 5),
                "sample-event",
                8,
                "sample@example.com");

            using var command = connection.CreateCommand();

            command.CommandText = "SELECT * FROM authentication_event WHERE id = @id";
            command.Parameters.AddWithValue("@id", id);

            using var reader = await command.ExecuteReaderAsync();

            await reader.ReadAsync();

            Assert.Equal(new(2000, 1, 2, 3, 4, 5), reader.GetDateTime("time"));
            Assert.Equal("sample-event", reader.GetString("event"));
            Assert.Equal(8, reader.GetInt64("user_id"));
            Assert.Equal("sample@example.com", reader.GetString("email"));
        }

        [Fact]
        public async Task LogEventAcceptsNullUserIdAndEmail()
        {
            using var connection = await TestDatabase.OpenConnectionWithRollback();

            var id = await new AuthenticationEventDataProvider().LogEvent(
                connection, DateTime.UtcNow, "sample-event");

            using var command = connection.CreateCommand();

            command.CommandText = "SELECT * FROM authentication_event WHERE id = @id";
            command.Parameters.AddWithValue("@id", id);

            using var reader = await command.ExecuteReaderAsync();

            await reader.ReadAsync();

            Assert.True(reader.IsDBNull(reader.GetOrdinal("user_id")));
            Assert.True(reader.IsDBNull(reader.GetOrdinal("email")));
        }

        #endregion
    }
}
