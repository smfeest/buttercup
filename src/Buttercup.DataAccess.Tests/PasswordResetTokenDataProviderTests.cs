using Buttercup.TestUtils;
using Microsoft.EntityFrameworkCore;
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
        using var dbContext = TestDatabase.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        var user = new ModelFactory().BuildUser();
        dbContext.Users.Add(user);

        void AddToken(string token, DateTime created) => dbContext.PasswordResetTokens.Add(new()
        { Token = token, UserId = user.Id, Created = created });

        AddToken("token-a", new(2000, 1, 2, 11, 59, 59));
        AddToken("token-b", new(2000, 1, 2, 12, 00, 00));
        AddToken("token-c", new(2000, 1, 2, 12, 00, 01));

        await dbContext.SaveChangesAsync();

        await this.passwordResetTokenDataProvider.DeleteExpiredTokens(
            dbContext, new(2000, 1, 2, 12, 00, 00));

        var survivingTokens =
            await dbContext.PasswordResetTokens.Select(x => x.Token).ToListAsync();

        Assert.Equal(new[] { "token-b", "token-c" }, survivingTokens);
    }

    #endregion

    #region DeleteTokensForUser

    [Fact]
    public async Task DeleteTokensForUserDeletesTokensBelongingToUser()
    {
        using var dbContext = TestDatabase.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        var modelFactory = new ModelFactory();

        var user = modelFactory.BuildUser();
        var otherUser = modelFactory.BuildUser();
        dbContext.Users.AddRange(user, otherUser);

        void AddToken(long userId, string token) => dbContext.PasswordResetTokens.Add(
            new() { Token = token, UserId = userId, Created = DateTime.UtcNow });

        AddToken(user.Id, "token-a");
        AddToken(otherUser.Id, "token-b");
        AddToken(user.Id, "token-c");

        await dbContext.SaveChangesAsync();

        await this.passwordResetTokenDataProvider.DeleteTokensForUser(dbContext, user.Id);

        var survivingToken = await dbContext.PasswordResetTokens.Select(x => x.Token).SingleAsync();

        Assert.Equal("token-b", survivingToken);
    }

    #endregion

    #region GetUserIdForToken

    [Fact]
    public async Task GetUserIdForTokenReturnsUserIdWhenTokenExists()
    {
        using var dbContext = TestDatabase.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        var user = new ModelFactory().BuildUser();
        dbContext.Users.Add(user);

        dbContext.PasswordResetTokens.Add(
            new() { UserId = user.Id, Token = "sample-token", Created = DateTime.UtcNow });

        await dbContext.SaveChangesAsync();

        var actual = await this.passwordResetTokenDataProvider.GetUserIdForToken(
            dbContext, "sample-token");

        Assert.Equal(user.Id, actual);
    }

    [Fact]
    public async Task GetUserIdForTokenReturnsNullIfNoMatchFound()
    {
        using var dbContext = TestDatabase.CreateDbContext();

        var actual = await this.passwordResetTokenDataProvider.GetUserIdForToken(
            dbContext, "sample-token");

        Assert.Null(actual);
    }

    #endregion

    #region InsertToken

    [Fact]
    public async Task InsertTokenInsertsToken()
    {
        using var dbContext = TestDatabase.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        var user = new ModelFactory().BuildUser();
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        await this.passwordResetTokenDataProvider.InsertToken(dbContext, user.Id, "sample-token");

        dbContext.ChangeTracker.Clear();

        var actual = await dbContext.PasswordResetTokens.FindAsync("sample-token");

        Assert.NotNull(actual);
        Assert.Equal(user.Id, actual.UserId);
        Assert.Equal(this.fakeTime, actual.Created);
    }

    #endregion
}
