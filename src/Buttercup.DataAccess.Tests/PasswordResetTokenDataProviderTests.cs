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

    #region GetUserIdForUnexpiredToken

    [Fact]
    public async Task GetUserIdForUnexpiredToken_ReturnsUserIdIfMatchingUnexpiredTokenExists()
    {
        using var dbContext = this.databaseFixture.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        var user = this.modelFactory.BuildUser();
        dbContext.Users.Add(user);

        var token = this.modelFactory.NextString("token");
        dbContext.PasswordResetTokens.Add(
            new() { UserId = user.Id, Token = token, Created = DateTime.UtcNow.AddSeconds(-99) });

        await dbContext.SaveChangesAsync();

        var actual = await this.passwordResetTokenDataProvider.GetUserIdForUnexpiredToken(
            dbContext, token, TimeSpan.FromSeconds(100));

        Assert.Equal(user.Id, actual);
    }

    [Fact]
    public async Task GetUserIdForUnexpiredToken_ReturnsNullIfTokenExpired()
    {
        using var dbContext = this.databaseFixture.CreateDbContext();

        var user = this.modelFactory.BuildUser();
        dbContext.Users.Add(user);

        var token = this.modelFactory.NextString("token");
        dbContext.PasswordResetTokens.Add(
            new() { UserId = user.Id, Token = token, Created = DateTime.UtcNow.AddSeconds(-101) });

        var actual = await this.passwordResetTokenDataProvider.GetUserIdForUnexpiredToken(
            dbContext, token, TimeSpan.FromSeconds(100));

        Assert.Null(actual);
    }

    [Fact]
    public async Task GetUserIdForUnexpiredToken_ReturnsNullIfTokenNotFound()
    {
        using var dbContext = this.databaseFixture.CreateDbContext();

        var actual = await this.passwordResetTokenDataProvider.GetUserIdForUnexpiredToken(
            dbContext, this.modelFactory.NextString("token"), new(2000, 1, 2, 12, 00, 00));

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
