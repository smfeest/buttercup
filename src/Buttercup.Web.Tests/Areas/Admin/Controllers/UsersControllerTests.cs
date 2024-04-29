using Buttercup.TestUtils;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Buttercup.Web.Areas.Admin.Controllers;

[Collection(nameof(DatabaseCollection))]
public sealed class UsersControllerTests(DatabaseFixture<DatabaseCollection> databaseFixture)
    : DatabaseTests<DatabaseCollection>(databaseFixture), IDisposable
{
    private readonly ModelFactory modelFactory = new();
    private readonly UsersController usersController = new(databaseFixture);

    public void Dispose() => this.usersController.Dispose();

    #region Index

    [Fact]
    public async Task Index_ReturnsViewResultWithUsersOrderedByNameAndEmail()
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

        var result = Assert.IsType<ViewResult>(await this.usersController.Index());

        Assert.Equal(orderedUsers, result.Model);
    }

    #endregion
}
