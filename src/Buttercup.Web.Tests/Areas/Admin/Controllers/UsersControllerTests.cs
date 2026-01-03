using Buttercup.TestUtils;
using Buttercup.Web.Areas.Admin.Controllers.Queries;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Buttercup.Web.Areas.Admin.Controllers;

[Collection(nameof(DatabaseCollection))]
public sealed class UsersControllerTests : IDisposable
{
    private readonly ModelFactory modelFactory = new();

    private readonly FakeDbContextFactory dbContextFactory = new();
    private readonly Mock<IUsersControllerQueries> queriesMock = new();
    private readonly UsersController usersController;

    public UsersControllerTests() =>
        this.usersController = new(this.dbContextFactory, this.queriesMock.Object);

    public void Dispose() => this.usersController.Dispose();

    #region Index

    [Fact]
    public async Task Index_ReturnsViewResultWithUsers()
    {
        var users = new[] { this.modelFactory.BuildUser() };
        this.queriesMock
            .Setup(x => x.GetUsersForIndex(this.dbContextFactory.FakeDbContext))
            .ReturnsAsync(users);

        var result = await this.usersController.Index();
        var viewResult = Assert.IsType<ViewResult>(result);

        Assert.Same(users, viewResult.Model);
    }

    #endregion
}
