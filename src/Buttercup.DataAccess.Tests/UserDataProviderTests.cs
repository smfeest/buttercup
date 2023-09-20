using System.Globalization;
using Buttercup.TestUtils;
using Xunit;

namespace Buttercup.DataAccess;

[Collection(nameof(DatabaseCollection))]
public sealed class UserDataProviderTests
{
    private readonly DatabaseCollectionFixture databaseFixture;
    private readonly ModelFactory modelFactory = new();

    private readonly StoppedClock clock = new();
    private readonly UserDataProvider userDataProvider;

    public UserDataProviderTests(DatabaseCollectionFixture databaseFixture)
    {
        this.databaseFixture = databaseFixture;
        this.userDataProvider = new(this.clock);
        this.clock.UtcNow = this.modelFactory.NextDateTime();
    }

    #region FindUserByEmail

    [Fact]
    public async Task FindUserByEmailReturnsUser()
    {
        using var dbContext = this.databaseFixture.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        var expected = this.modelFactory.BuildUser();
        dbContext.Users.Add(expected);
        await dbContext.SaveChangesAsync();

        var actual = await this.userDataProvider.FindUserByEmail(dbContext, expected.Email);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task FindUserByEmailReturnsNullIfNoMatchFound()
    {
        using var dbContext = this.databaseFixture.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        dbContext.Users.Add(this.modelFactory.BuildUser());
        await dbContext.SaveChangesAsync();

        var actual = await this.userDataProvider.FindUserByEmail(
            dbContext, this.modelFactory.NextEmail());

        Assert.Null(actual);
    }

    #endregion

    #region GetUser

    [Fact]
    public async Task GetUserReturnsUser()
    {
        using var dbContext = this.databaseFixture.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        var expected = this.modelFactory.BuildUser();
        dbContext.Users.Add(expected);
        await dbContext.SaveChangesAsync();

        dbContext.ChangeTracker.Clear();

        var actual = await this.userDataProvider.GetUser(dbContext, expected.Id);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task GetUserThrowsIfRecordNotFound()
    {
        using var dbContext = this.databaseFixture.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        dbContext.Users.Add(this.modelFactory.BuildUser());
        await dbContext.SaveChangesAsync();

        var id = this.modelFactory.NextInt();

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => this.userDataProvider.GetUser(dbContext, id));

        Assert.Equal($"User {id} not found", exception.Message);
    }

    #endregion

    #region UpdatePassword

    [Fact]
    public async Task UpdatePasswordUpdatesHashedPassword()
    {
        using var dbContext = this.databaseFixture.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        var original = this.modelFactory.BuildUser();
        dbContext.Users.Add(original);
        await dbContext.SaveChangesAsync();

        var newHashedPassword = this.modelFactory.NextString("hashed-password");
        var newSecurityStamp = this.NextSecurityStamp();

        await this.userDataProvider.UpdatePassword(
            dbContext, original.Id, newHashedPassword, newSecurityStamp);

        dbContext.ChangeTracker.Clear();

        var expected = original with
        {
            HashedPassword = newHashedPassword,
            PasswordCreated = this.clock.UtcNow,
            SecurityStamp = newSecurityStamp,
            Modified = this.clock.UtcNow,
            Revision = original.Revision + 1,
        };

        var actual = await dbContext.Users.FindAsync(original.Id);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task UpdatePasswordThrowsIfRecordNotFound()
    {
        using var dbContext = this.databaseFixture.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        dbContext.Users.Add(this.modelFactory.BuildUser());
        await dbContext.SaveChangesAsync();

        var id = this.modelFactory.NextInt();

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => this.userDataProvider.UpdatePassword(
                dbContext,
                id,
                this.modelFactory.NextString("hashed-password"),
                this.NextSecurityStamp()));

        Assert.Equal($"User {id} not found", exception.Message);
    }

    private string NextSecurityStamp() =>
        this.modelFactory.NextInt().ToString("X8", CultureInfo.InvariantCulture);

    #endregion

    #region UpdatePreferences

    [Fact]
    public async Task UpdatePreferencesUpdatesPreferences()
    {
        using var dbContext = this.databaseFixture.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        var original = this.modelFactory.BuildUser();
        dbContext.Users.Add(original);
        await dbContext.SaveChangesAsync();

        var newTimeZone = this.modelFactory.NextString("new-time-zone");

        await this.userDataProvider.UpdatePreferences(dbContext, original.Id, newTimeZone);

        dbContext.ChangeTracker.Clear();

        var expected = original with
        {
            TimeZone = newTimeZone,
            Modified = this.clock.UtcNow,
            Revision = original.Revision + 1,
        };

        var actual = await dbContext.Users.FindAsync(original.Id);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task UpdatePreferencesThrowsIfRecordNotFound()
    {
        using var dbContext = this.databaseFixture.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        dbContext.Users.Add(this.modelFactory.BuildUser());
        await dbContext.SaveChangesAsync();

        var id = this.modelFactory.NextInt();

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => this.userDataProvider.UpdatePreferences(
                dbContext, id, this.modelFactory.NextString("time-zone")));

        Assert.Equal($"User {id} not found", exception.Message);
    }

    #endregion
}
