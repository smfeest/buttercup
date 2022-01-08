using Buttercup.TestUtils;
using Moq;
using Xunit;

namespace Buttercup.DataAccess;

[Collection("Database collection")]
public class PasswordResetTokenDataProviderTests
{
    private readonly DateTime fakeTime = new(2020, 1, 2, 3, 4, 5);
    private readonly PasswordResetTokenDataProvider passwordResetTokenDataProvider;

    public PasswordResetTokenDataProviderTests()
    {
        var clock = Mock.Of<IClock>(x => x.UtcNow == this.fakeTime);

        this.passwordResetTokenDataProvider = new(clock);
    }

    #region DeleteExpiredTokens

    [Fact]
    public async Task DeleteExpiredTokensDeletesExpiredTokens()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        await new SampleDataHelper(connection).InsertUser(ModelFactory.CreateUser(id: 3));

        async Task InsertToken(string token, DateTime created)
        {
            using var command = connection.CreateCommand();

            command.CommandText = @"INSERT password_reset_token(token, user_id, created)
                VALUES (@token, 3, @created)";

            command.Parameters.AddWithValue("@token", token);
            command.Parameters.AddWithValue("@created", created);

            await command.ExecuteNonQueryAsync();
        }

        await InsertToken("token-a", new(2000, 1, 2, 11, 59, 59));
        await InsertToken("token-b", new(2000, 1, 2, 12, 00, 00));
        await InsertToken("token-c", new(2000, 1, 2, 12, 00, 01));

        await this.passwordResetTokenDataProvider.DeleteExpiredTokens(
            connection, new(2000, 1, 2, 12, 00, 00));

        string? survivingTokens;

        using (var command = connection.CreateCommand())
        {
            command.CommandText = "SELECT GROUP_CONCAT(token) FROM password_reset_token";
            survivingTokens = (string?)await command.ExecuteScalarAsync();
        }

        Assert.Equal("token-b,token-c", survivingTokens);
    }

    #endregion

    #region DeleteTokensForUser

    [Fact]
    public async Task DeleteTokensForUserDeletesTokensBelongingToUser()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        var sampleDataHelper = new SampleDataHelper(connection);

        await sampleDataHelper.InsertUser(ModelFactory.CreateUser(id: 7));
        await sampleDataHelper.InsertUser(ModelFactory.CreateUser(id: 11));

        await this.passwordResetTokenDataProvider.InsertToken(connection, 7, "token-a");
        await this.passwordResetTokenDataProvider.InsertToken(connection, 11, "token-b");
        await this.passwordResetTokenDataProvider.InsertToken(connection, 7, "token-c");

        await this.passwordResetTokenDataProvider.DeleteTokensForUser(connection, 7);

        string? survivingTokens;

        using (var command = connection.CreateCommand())
        {
            command.CommandText = "SELECT GROUP_CONCAT(token) FROM password_reset_token";
            survivingTokens = (string?)await command.ExecuteScalarAsync();
        }

        Assert.Equal("token-b", survivingTokens);
    }

    #endregion

    #region GetUserIdForToken

    [Fact]
    public async Task GetUserIdForTokenReturnsUserIdWhenTokenExists()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        await new SampleDataHelper(connection).InsertUser(ModelFactory.CreateUser(id: 5));
        await this.passwordResetTokenDataProvider.InsertToken(connection, 5, "sample-token");

        var actual = await this.passwordResetTokenDataProvider.GetUserIdForToken(
            connection, "sample-token");

        Assert.Equal(5, actual);
    }

    [Fact]
    public async Task GetUserIdForTokenReturnsNullIfNoMatchFound()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        var actual = await this.passwordResetTokenDataProvider.GetUserIdForToken(
            connection, "sample-token");

        Assert.Null(actual);
    }

    #endregion

    #region InsertToken

    [Fact]
    public async Task InsertTokenInsertsToken()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        await new SampleDataHelper(connection).InsertUser(ModelFactory.CreateUser(id: 6));

        await this.passwordResetTokenDataProvider.InsertToken(connection, 6, "sample-token");

        using var command = connection.CreateCommand();

        command.CommandText = "SELECT * FROM password_reset_token WHERE token = 'sample-token'";

        using var reader = await command.ExecuteReaderAsync();

        await reader.ReadAsync();

        Assert.Equal(6, reader.GetInt64("user_id"));
        Assert.Equal(this.fakeTime, reader.GetDateTime("created"));
    }

    #endregion
}
