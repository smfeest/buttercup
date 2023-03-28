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

        var user = await new SampleDataHelper(connection).InsertUser();

        async Task InsertToken(string token, DateTime created)
        {
            using var command = connection.CreateCommand();

            command.CommandText = @"INSERT password_reset_tokens(token, user_id, created)
                VALUES (@token, @user_id, @created)";

            command.Parameters.AddWithValue("@token", token);
            command.Parameters.AddWithValue("@user_id", user.Id);
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
            command.CommandText = "SELECT GROUP_CONCAT(token) FROM password_reset_tokens";
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

        var user = await sampleDataHelper.InsertUser();
        var otherUser = await sampleDataHelper.InsertUser();

        await this.passwordResetTokenDataProvider.InsertToken(connection, user.Id, "token-a");
        await this.passwordResetTokenDataProvider.InsertToken(connection, otherUser.Id, "token-b");
        await this.passwordResetTokenDataProvider.InsertToken(connection, user.Id, "token-c");

        await this.passwordResetTokenDataProvider.DeleteTokensForUser(connection, user.Id);

        string? survivingTokens;

        using (var command = connection.CreateCommand())
        {
            command.CommandText = "SELECT GROUP_CONCAT(token) FROM password_reset_tokens";
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

        var user = await new SampleDataHelper(connection).InsertUser();

        await this.passwordResetTokenDataProvider.InsertToken(connection, user.Id, "sample-token");

        var actual = await this.passwordResetTokenDataProvider.GetUserIdForToken(
            connection, "sample-token");

        Assert.Equal(user.Id, actual);
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

        var user = await new SampleDataHelper(connection).InsertUser();

        await this.passwordResetTokenDataProvider.InsertToken(connection, user.Id, "sample-token");

        using var command = connection.CreateCommand();

        command.CommandText = "SELECT * FROM password_reset_tokens WHERE token = 'sample-token'";

        using var reader = await command.ExecuteReaderAsync();

        await reader.ReadAsync();

        Assert.Equal(user.Id, reader.GetInt64("user_id"));
        Assert.Equal(this.fakeTime, reader.GetDateTime("created"));
    }

    #endregion
}
