using Buttercup.TestUtils;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Buttercup.DataAccess;

[Collection(nameof(DatabaseCollection))]
public sealed class PasswordResetTokenDataProviderTests
{
    private readonly DatabaseCollectionFixture databaseFixture;
    private readonly ModelFactory modelFactory = new();

    private readonly StoppedClock clock = new();
    private readonly PasswordResetTokenDataProvider passwordResetTokenDataProvider;

    public PasswordResetTokenDataProviderTests(DatabaseCollectionFixture databaseFixture)
    {
        this.databaseFixture = databaseFixture;
        this.passwordResetTokenDataProvider = new(this.clock);
        this.clock.UtcNow = this.modelFactory.NextDateTime();
    }

    #region DeleteExpiredTokens

    [Fact]
    public async Task DeleteExpiredTokens_DeletesExpiredTokens()
    {
        using var dbContext = this.databaseFixture.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        var user = this.modelFactory.BuildUser();
        dbContext.Users.Add(user);

        void AddToken(string token, DateTime created) => dbContext.PasswordResetTokens.Add(
            new() { Token = token, UserId = user.Id, Created = created });

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
    public async Task DeleteTokensForUser_DeletesTokensBelongingToUser()
    {
        using var dbContext = this.databaseFixture.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        var user = this.modelFactory.BuildUser();
        var otherUser = this.modelFactory.BuildUser();
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
    public async Task GetUserIdForToken_ReturnsUserIdWhenTokenExists()
    {
        using var dbContext = this.databaseFixture.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        var user = this.modelFactory.BuildUser();
        dbContext.Users.Add(user);

        var token = this.modelFactory.NextString("token");

        dbContext.PasswordResetTokens.Add(
            new() { UserId = user.Id, Token = token, Created = DateTime.UtcNow });

        await dbContext.SaveChangesAsync();

        var actual = await this.passwordResetTokenDataProvider.GetUserIdForToken(dbContext, token);

        Assert.Equal(user.Id, actual);
    }

    [Fact]
    public async Task GetUserIdForToken_ReturnsNullIfNoMatchFound()
    {
        using var dbContext = this.databaseFixture.CreateDbContext();

        var actual = await this.passwordResetTokenDataProvider.GetUserIdForToken(
            dbContext, this.modelFactory.NextString("token"));

        Assert.Null(actual);
    }

    #endregion

    #region InsertToken

    [Fact]
    public async Task InsertToken_InsertsToken()
    {
        using var dbContext = this.databaseFixture.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        var user = this.modelFactory.BuildUser();
        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var token = this.modelFactory.NextString("token");

        await this.passwordResetTokenDataProvider.InsertToken(dbContext, user.Id, token);

        dbContext.ChangeTracker.Clear();

        var actual = await dbContext.PasswordResetTokens.FindAsync(token);

        Assert.NotNull(actual);
        Assert.Equal(user.Id, actual.UserId);
        Assert.Equal(this.clock.UtcNow, actual.Created);
    }

    #endregion
}
