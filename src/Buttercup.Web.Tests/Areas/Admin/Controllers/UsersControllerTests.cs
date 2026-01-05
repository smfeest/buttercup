using Buttercup.Application;
using Buttercup.EntityModel;
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
    private readonly Mock<IUserManager> userManagerMock = new();

    private readonly UsersController usersController;

    public UsersControllerTests() =>
        this.usersController = new(
            this.dbContextFactory, this.queriesMock.Object, this.userManagerMock.Object);

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

    #region Show

    [Fact]
    public async Task Show_Success_ReturnsViewResultWithUser()
    {
        var user = this.modelFactory.BuildUser();

        this.userManagerMock.Setup(x => x.FindUser(user.Id)).ReturnsAsync(user);

        var result = await this.usersController.Show(user.Id);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(user, viewResult.Model);
    }

    [Fact]
    public async Task Show_UserNotFound_ReturnsNotFoundResult()
    {
        var userId = this.modelFactory.NextInt();
        this.userManagerMock.Setup(x => x.FindUser(userId)).ReturnsAsync(default(User?));

        var result = await this.usersController.Show(userId);
        Assert.IsType<NotFoundResult>(result);
    }

    #endregion
}
