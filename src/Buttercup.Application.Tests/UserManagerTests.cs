using Buttercup.EntityModel;
using Buttercup.TestUtils;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace Buttercup.Application;

[Collection(nameof(DatabaseCollection))]
public sealed class UserManagerTests : DatabaseTests<DatabaseCollection>
{
    private readonly ModelFactory modelFactory = new();

    private readonly FakeTimeProvider timeProvider;
    private readonly UserManager userManager;

    public UserManagerTests(DatabaseFixture<DatabaseCollection> databaseFixture)
        : base(databaseFixture)
    {
        this.timeProvider = new(this.modelFactory.NextDateTime());
        this.userManager = new(this.DatabaseFixture, this.timeProvider);
    }

    #region GetUser

    [Fact]
    public async Task GetUser_ReturnsUser()
    {
        var expected = this.modelFactory.BuildUser();
        await this.DatabaseFixture.InsertEntities(expected);

        // Returns user
        Assert.Equal(expected, await this.userManager.FindUser(expected.Id));
    }

    [Fact]
    public async Task GetUser_UserDoesNotExist()
    {
        await this.DatabaseFixture.InsertEntities(this.modelFactory.BuildUser());

        // Returns null
        Assert.Null(await this.userManager.FindUser(this.modelFactory.NextInt()));
    }

    #endregion

    #region SetTimeZone

    [Fact]
    public async Task SetTimeZone_Success()
    {
        var original = this.modelFactory.BuildUser();
        await this.DatabaseFixture.InsertEntities(original);

        var newTimeZone = this.modelFactory.NextString("new-time-zone");
        await this.userManager.SetTimeZone(original.Id, newTimeZone);

        using var dbContext = this.DatabaseFixture.CreateDbContext();

        // Updates user
        var expected = original with
        {
            TimeZone = newTimeZone,
            Modified = this.timeProvider.GetUtcDateTimeNow(),
            Revision = original.Revision + 1,
        };
        var actual = await dbContext.Users.FindAsync(
            [original.Id], TestContext.Current.CancellationToken);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task SetTimeZone_UserDoesNotExist()
    {
        await this.DatabaseFixture.InsertEntities(this.modelFactory.BuildUser());
        var id = this.modelFactory.NextInt();

        // Throws exception
        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => this.userManager.SetTimeZone(id, this.modelFactory.NextString("time-zone")));
        Assert.Equal($"User {id} not found", exception.Message);
    }

    #endregion
}
