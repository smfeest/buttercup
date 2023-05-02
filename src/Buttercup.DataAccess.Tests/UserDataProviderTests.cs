using Buttercup.TestUtils;
using Moq;
using Xunit;

namespace Buttercup.DataAccess;

[Collection(nameof(DatabaseCollection))]
public class UserDataProviderTests
{
    private readonly DateTime fakeTime = new(2020, 1, 2, 3, 4, 5);
    private readonly UserDataProvider userDataProvider;

    public UserDataProviderTests()
    {
        var clock = Mock.Of<IClock>(x => x.UtcNow == this.fakeTime);

        this.userDataProvider = new(clock);
    }

    #region FindUserByEmail

    [Fact]
    public async Task FindUserByEmailReturnsUser()
    {
        using var dbContext = TestDatabase.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        var expected = new ModelFactory().BuildUser();
        dbContext.Users.Add(expected);
        await dbContext.SaveChangesAsync();

        var actual = await this.userDataProvider.FindUserByEmail(dbContext, expected.Email);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task FindUserByEmailReturnsNullIfNoMatchFound()
    {
        using var dbContext = TestDatabase.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        dbContext.Users.Add(new ModelFactory().BuildUser());
        await dbContext.SaveChangesAsync();

        var actual = await this.userDataProvider.FindUserByEmail(
            dbContext, "non-existent@example.com");

        Assert.Null(actual);
    }

    #endregion

    #region GetUser

    [Fact]
    public async Task GetUserReturnsUser()
    {
        using var dbContext = TestDatabase.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        var expected = new ModelFactory().BuildUser();
        dbContext.Users.Add(expected);
        await dbContext.SaveChangesAsync();

        dbContext.ChangeTracker.Clear();

        var actual = await this.userDataProvider.GetUser(dbContext, expected.Id);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task GetUserThrowsIfRecordNotFound()
    {
        using var dbContext = TestDatabase.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        var modelFactory = new ModelFactory();

        dbContext.Users.Add(modelFactory.BuildUser());
        await dbContext.SaveChangesAsync();

        var id = modelFactory.NextInt();

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => this.userDataProvider.GetUser(dbContext, id));

        Assert.Equal($"User {id} not found", exception.Message);
    }

    #endregion

    #region GetUsers

    [Fact]
    public async Task GetUsersReturnsUsersWithMatchingIds()
    {
        using var dbContext = TestDatabase.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        var modelFactory = new ModelFactory();

        var allUsers = new[]
        {
            modelFactory.BuildUser(),
            modelFactory.BuildUser(),
            modelFactory.BuildUser(),
        };

        dbContext.Users.AddRange(allUsers);
        await dbContext.SaveChangesAsync();

        var expected = new[] { allUsers[0], allUsers[2] };

        var actual = await this.userDataProvider.GetUsers(
            dbContext, expected.Select(u => u.Id).ToArray());

        Assert.Equal(expected, actual);
    }

    #endregion

    #region UpdatePassword

    [Fact]
    public async Task UpdatePasswordUpdatesHashedPassword()
    {
        using var dbContext = TestDatabase.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        var original = new ModelFactory().BuildUser();
        dbContext.Users.Add(original);
        await dbContext.SaveChangesAsync();

        await this.userDataProvider.UpdatePassword(
            dbContext, original.Id, "new-hashed-password", "newstamp");

        dbContext.ChangeTracker.Clear();

        var expected = original with
        {
            HashedPassword = "new-hashed-password",
            PasswordCreated = this.fakeTime,
            SecurityStamp = "newstamp",
            Modified = this.fakeTime,
            Revision = original.Revision + 1,
        };

        var actual = await dbContext.Users.FindAsync(original.Id);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task UpdatePasswordThrowsIfRecordNotFound()
    {
        using var dbContext = TestDatabase.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        var modelFactory = new ModelFactory();

        dbContext.Users.Add(modelFactory.BuildUser());
        await dbContext.SaveChangesAsync();

        var id = modelFactory.NextInt();

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => this.userDataProvider.UpdatePassword(
                dbContext, id, "new-hashed-password", "newstamp"));

        Assert.Equal($"User {id} not found", exception.Message);
    }

    #endregion

    #region UpdatePreferences

    [Fact]
    public async Task UpdatePreferencesUpdatesPreferences()
    {
        using var dbContext = TestDatabase.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        var original = new ModelFactory().BuildUser();
        dbContext.Users.Add(original);
        await dbContext.SaveChangesAsync();

        await this.userDataProvider.UpdatePreferences(dbContext, original.Id, "new-time-zone");

        dbContext.ChangeTracker.Clear();

        var expected = original with
        {
            TimeZone = "new-time-zone",
            Modified = this.fakeTime,
            Revision = original.Revision + 1,
        };

        var actual = await dbContext.Users.FindAsync(original.Id);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task UpdatePreferencesThrowsIfRecordNotFound()
    {
        using var dbContext = TestDatabase.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        var modelFactory = new ModelFactory();

        dbContext.Users.Add(modelFactory.BuildUser());
        await dbContext.SaveChangesAsync();

        var id = modelFactory.NextInt();

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => this.userDataProvider.UpdatePreferences(dbContext, id, "new-time-zone"));

        Assert.Equal($"User {id} not found", exception.Message);
    }

    #endregion
}
