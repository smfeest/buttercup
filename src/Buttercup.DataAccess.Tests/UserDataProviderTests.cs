using Buttercup.EntityModel;
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

    #region GetUser

    [Fact]
    public async Task GetUser_ReturnsUser()
    {
        using var dbContext = this.databaseFixture.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        var expected = this.modelFactory.BuildUser();
        dbContext.Users.Add(expected);
        await dbContext.SaveChangesAsync();

        dbContext.ChangeTracker.Clear();

        // Returns user
        var actual = await this.userDataProvider.GetUser(dbContext, expected.Id);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task GetUser_UserDoesNotExist()
    {
        using var dbContext = this.databaseFixture.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        dbContext.Users.Add(this.modelFactory.BuildUser());
        await dbContext.SaveChangesAsync();

        var id = this.modelFactory.NextInt();

        // Throws exception
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => this.userDataProvider.GetUser(dbContext, id));
        Assert.Equal($"User {id} not found", exception.Message);
    }

    #endregion

    #region SetTimeZone

    [Fact]
    public async Task SetTimeZone_Success()
    {
        using var dbContext = this.databaseFixture.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        var original = this.modelFactory.BuildUser();
        dbContext.Users.Add(original);
        await dbContext.SaveChangesAsync();

        var newTimeZone = this.modelFactory.NextString("new-time-zone");

        await this.userDataProvider.SetTimeZone(dbContext, original.Id, newTimeZone);

        dbContext.ChangeTracker.Clear();

        // Updates user
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
    public async Task SetTimeZone_UserDoesNotExist()
    {
        using var dbContext = this.databaseFixture.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        dbContext.Users.Add(this.modelFactory.BuildUser());
        await dbContext.SaveChangesAsync();

        var id = this.modelFactory.NextInt();

        // Throws exception
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => this.userDataProvider.SetTimeZone(
                dbContext, id, this.modelFactory.NextString("time-zone")));
        Assert.Equal($"User {id} not found", exception.Message);
    }

    #endregion
}
