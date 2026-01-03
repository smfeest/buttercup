using Buttercup.TestUtils;
using Xunit;

namespace Buttercup.Web.Areas.Admin.Controllers.Queries;

[Collection(nameof(DatabaseCollection))]
public sealed class UsersControllerQueriesTests(
    DatabaseFixture<DatabaseCollection> databaseFixture)
    : DatabaseTests<DatabaseCollection>(databaseFixture)
{
    private readonly ModelFactory modelFactory = new();
    private readonly UsersControllerQueries queries = new();

    #region GetUsersForIndex

    [Fact]
    public async Task GetUsersForIndex_ReturnsAllUsersOrderedByNameAndEmail()
    {
        var orderedUsers = new[]
        {
            this.modelFactory.BuildUser() with { Name = "Anna", Email = "w@example.com" },
            this.modelFactory.BuildUser() with { Name = "Anna", Email = "z@example.com" },
            this.modelFactory.BuildUser() with { Name = "Bob", Email = "x@example.com" },
            this.modelFactory.BuildUser() with { Name = "Clara", Email = "y@example.com" },
        };
        await this.DatabaseFixture.InsertEntities(
            orderedUsers[1], orderedUsers[3], orderedUsers[2], orderedUsers[0]);

        var result = await this.queries.GetUsersForIndex(this.DatabaseFixture.CreateDbContext());

        Assert.Equal(orderedUsers, result);
    }

    #endregion
}
