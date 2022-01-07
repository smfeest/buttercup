using Moq;
using Xunit;

namespace Buttercup.DataAccess;

[Collection("Database collection")]
public class AuthenticationEventDataProviderTests
{
    private readonly DateTime fakeTime = new(2020, 1, 2, 3, 4, 5);
    private readonly AuthenticationEventDataProvider authenticationEventDataProvider;

    public AuthenticationEventDataProviderTests()
    {
        var clock = Mock.Of<IClock>(x => x.UtcNow == this.fakeTime);

        this.authenticationEventDataProvider = new(clock);
    }

    #region LogEvent

    [Fact]
    public async Task LogEventInsertsEvent()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        await SampleUsers.InsertSampleUser(connection, SampleUsers.CreateSampleUser(id: 8));

        var id = await this.authenticationEventDataProvider.LogEvent(
            connection, "sample-event", 8, "sample@example.com");

        using var command = connection.CreateCommand();

        command.CommandText = "SELECT * FROM authentication_event WHERE id = @id";
        command.Parameters.AddWithValue("@id", id);

        using var reader = await command.ExecuteReaderAsync();

        await reader.ReadAsync();

        Assert.Equal(this.fakeTime, reader.GetDateTime("time"));
        Assert.Equal("sample-event", reader.GetString("event"));
        Assert.Equal(8, reader.GetInt64("user_id"));
        Assert.Equal("sample@example.com", reader.GetString("email"));
    }

    [Fact]
    public async Task LogEventAcceptsNullUserIdAndEmail()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        var id = await this.authenticationEventDataProvider.LogEvent(connection, "sample-event");

        using var command = connection.CreateCommand();

        command.CommandText = "SELECT * FROM authentication_event WHERE id = @id";
        command.Parameters.AddWithValue("@id", id);

        using var reader = await command.ExecuteReaderAsync();

        await reader.ReadAsync();

        Assert.True(await reader.IsDBNullAsync(reader.GetOrdinal("user_id")));
        Assert.True(await reader.IsDBNullAsync(reader.GetOrdinal("email")));
    }

    #endregion
}
