using Buttercup.EntityModel;
using Buttercup.TestUtils;
using Xunit;

namespace Buttercup.Application;

[Collection(nameof(DatabaseCollection))]
public sealed class UserManagerTests : IAsyncLifetime
{
    private readonly DatabaseCollectionFixture databaseFixture;
    private readonly ModelFactory modelFactory = new();

    private readonly StoppedClock clock = new();
    private readonly UserManager userManager;

    public UserManagerTests(DatabaseCollectionFixture databaseFixture)
    {
        this.databaseFixture = databaseFixture;
        this.userManager = new(this.clock, this.databaseFixture);
        this.clock.UtcNow = this.modelFactory.NextDateTime();
    }

    public Task InitializeAsync() => this.databaseFixture.ClearDatabase();

    public Task DisposeAsync() => Task.CompletedTask;

    #region GetUser

    [Fact]
    public async Task GetUser_ReturnsUser()
    {
        var expected = this.modelFactory.BuildUser();

        using (var dbContext = this.databaseFixture.CreateDbContext())
        {
            dbContext.Users.Add(expected);
            await dbContext.SaveChangesAsync();
        }

        // Returns user
        Assert.Equal(expected, await this.userManager.GetUser(expected.Id));
    }

    [Fact]
    public async Task GetUser_UserDoesNotExist()
    {
        using (var dbContext = this.databaseFixture.CreateDbContext())
        {
            dbContext.Users.Add(this.modelFactory.BuildUser());
            await dbContext.SaveChangesAsync();
        }

        var id = this.modelFactory.NextInt();

        // Throws exception
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => this.userManager.GetUser(id));
        Assert.Equal($"User {id} not found", exception.Message);
    }

    #endregion

    #region SetTimeZone

    [Fact]
    public async Task SetTimeZone_Success()
    {
        var original = this.modelFactory.BuildUser();

        using (var dbContext = this.databaseFixture.CreateDbContext())
        {
            dbContext.Users.Add(original);
            await dbContext.SaveChangesAsync();
        }

        var newTimeZone = this.modelFactory.NextString("new-time-zone");

        await this.userManager.SetTimeZone(original.Id, newTimeZone);

        using (var dbContext = this.databaseFixture.CreateDbContext())
        {
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
    }

    [Fact]
    public async Task SetTimeZone_UserDoesNotExist()
    {
        using (var dbContext = this.databaseFixture.CreateDbContext())
        {
            dbContext.Users.Add(this.modelFactory.BuildUser());
            await dbContext.SaveChangesAsync();
        }

        var id = this.modelFactory.NextInt();

        // Throws exception
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => this.userManager.SetTimeZone(id, this.modelFactory.NextString("time-zone")));
        Assert.Equal($"User {id} not found", exception.Message);
    }

    #endregion
}
