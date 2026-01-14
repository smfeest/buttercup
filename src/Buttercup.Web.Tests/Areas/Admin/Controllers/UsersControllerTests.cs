using System.Net;
using Buttercup.Application;
using Buttercup.EntityModel;
using Buttercup.TestUtils;
using Buttercup.Web.Areas.Admin.Controllers.Queries;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Buttercup.Web.Areas.Admin.Controllers;

[Collection(nameof(DatabaseCollection))]
public sealed class UsersControllerTests : IDisposable
{
    private readonly ModelFactory modelFactory = new();

    private readonly FakeDbContextFactory dbContextFactory = new();
    private readonly DefaultHttpContext httpContext = new();
    private readonly DictionaryLocalizer<UsersController> localizer = new();
    private readonly Mock<IUsersControllerQueries> queriesMock = new();
    private readonly Mock<IUserManager> userManagerMock = new();

    private readonly UsersController usersController;

    public UsersControllerTests() =>
        this.usersController = new(
            this.dbContextFactory,
            this.localizer,
            this.queriesMock.Object,
            this.userManagerMock.Object)
        {
            ControllerContext = new() { HttpContext = this.httpContext },
        };

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

    #region New (GET)

    [Fact]
    public void New_Get_ReturnsViewResultWithDefaultTimeZone()
    {
        var result = this.usersController.New();
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(new NewUserAttributes { TimeZone = "Europe/London" }, viewResult.Model);
    }

    #endregion

    #region New (POST)

    [Fact]
    public async Task New_Post_Success_CreatesUser()
    {
        var attributes = this.BuildNewUserAttributes();
        var currentUserId = this.SetupCurrentUserId();
        var ipAddress = this.SetupRemoteIpAddress();

        var result = await this.usersController.New(attributes);

        this.userManagerMock.Verify(x => x.CreateUser(attributes, currentUserId, ipAddress));
    }

    [Fact]
    public async Task New_Post_Success_RedirectsToIndex()
    {
        this.SetupCurrentUserId();

        var result = await this.usersController.New(this.BuildNewUserAttributes());

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(UsersController.Index), redirectResult.ActionName);
    }

    [Fact]
    public async Task New_Post_InvalidModel_ReturnsViewResultWithModel()
    {
        var attributes = this.BuildNewUserAttributes();

        this.usersController.ModelState.AddModelError("test", "test");

        var result = await this.usersController.New(attributes);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(attributes, viewResult.Model);
    }

    [Fact]
    public async Task New_Post_EmailNotUnique_AddsErrorToModelState()
    {
        var attributes = this.BuildNewUserAttributes();
        var currentUserId = this.SetupCurrentUserId();
        var ipAddress = this.SetupRemoteIpAddress();

        this.localizer.Add("Error_EmailNotUnique", "translated-email-not-unique-error");

        this.userManagerMock
            .Setup(x => x.CreateUser(attributes, currentUserId, ipAddress))
            .ThrowsAsync(new NotUniqueException(nameof(NewUserAttributes.Email)));

        await this.usersController.New(attributes);

        var formState = this.usersController.ModelState[nameof(NewUserAttributes.Email)];
        Assert.NotNull(formState);

        var error = Assert.Single(formState.Errors);
        Assert.Equal("translated-email-not-unique-error", error.ErrorMessage);
    }

    [Fact]
    public async Task New_Post_EmailNotUnique_ReturnsViewResultWithModel()
    {
        var attributes = this.BuildNewUserAttributes();
        var currentUserId = this.SetupCurrentUserId();
        var ipAddress = this.SetupRemoteIpAddress();

        this.userManagerMock
            .Setup(x => x.CreateUser(attributes, currentUserId, ipAddress))
            .ThrowsAsync(new NotUniqueException(nameof(NewUserAttributes.Email)));

        var result = await this.usersController.New(attributes);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Null(viewResult.ViewName);
        Assert.Same(attributes, viewResult.Model);
    }

    [Fact]
    public async Task New_Post_NotUniqueExceptionForOtherProperty_IsNotCaught()
    {
        var attributes = this.BuildNewUserAttributes();
        var currentUserId = this.SetupCurrentUserId();
        var ipAddress = this.SetupRemoteIpAddress();

        this.userManagerMock
            .Setup(x => x.CreateUser(attributes, currentUserId, ipAddress))
            .ThrowsAsync(new NotUniqueException("Foo"));

        await Assert.ThrowsAsync<NotUniqueException>(
            () => this.usersController.New(attributes));
    }

    private NewUserAttributes BuildNewUserAttributes() =>
        new()
        {
            Name = this.modelFactory.NextString("name"),
            Email = this.modelFactory.NextEmail(),
            TimeZone = this.modelFactory.NextString("time-zone"),
        };

    #endregion

    #region Deactivate

    [Fact]
    public async Task Deactivate_Success_DeactivatesUser()
    {
        var userId = this.modelFactory.NextInt();
        var currentUserId = this.SetupCurrentUserId();
        var ipAddress = this.SetupRemoteIpAddress();

        await this.usersController.Deactivate(userId);

        this.userManagerMock.Verify(x =>
            x.DeactivateUser(userId, currentUserId, ipAddress));
    }

    [Fact]
    public async Task Deactivate_Success_RedirectsToShow()
    {
        long userId = this.modelFactory.NextInt();
        this.SetupCurrentUserId();

        var result = await this.usersController.Deactivate(userId);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(UsersController.Show), redirectResult.ActionName);
        Assert.NotNull(redirectResult.RouteValues);
        Assert.Equal(userId, redirectResult.RouteValues["id"]);
    }

    [Fact]
    public async Task Deactivate_UserNotFound_ReturnsNotFoundResult()
    {
        var userId = this.modelFactory.NextInt();
        var currentUserId = this.SetupCurrentUserId();

        this.userManagerMock
            .Setup(x => x.DeactivateUser(userId, currentUserId, null))
            .ThrowsAsync(new NotFoundException());

        var result = await this.usersController.Deactivate(userId);

        Assert.IsType<NotFoundResult>(result);
    }

    #endregion

    #region Reactivate

    [Fact]
    public async Task Reactivate_Success_ReactivatesUser()
    {
        var userId = this.modelFactory.NextInt();
        var currentUserId = this.SetupCurrentUserId();
        var ipAddress = this.SetupRemoteIpAddress();

        await this.usersController.Reactivate(userId);

        this.userManagerMock.Verify(x =>
            x.ReactivateUser(userId, currentUserId, ipAddress));
    }

    [Fact]
    public async Task Reactivate_Success_RedirectsToShow()
    {
        long userId = this.modelFactory.NextInt();
        this.SetupCurrentUserId();

        var result = await this.usersController.Reactivate(userId);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(UsersController.Show), redirectResult.ActionName);
        Assert.NotNull(redirectResult.RouteValues);
        Assert.Equal(userId, redirectResult.RouteValues["id"]);
    }

    [Fact]
    public async Task Reactivate_UserNotFound_ReturnsNotFoundResult()
    {
        var userId = this.modelFactory.NextInt();
        var currentUserId = this.SetupCurrentUserId();

        this.userManagerMock
            .Setup(x => x.ReactivateUser(userId, currentUserId, null))
            .ThrowsAsync(new NotFoundException());

        var result = await this.usersController.Reactivate(userId);

        Assert.IsType<NotFoundResult>(result);
    }

    #endregion

    private long SetupCurrentUserId()
    {
        var userId = this.modelFactory.NextInt();
        this.httpContext.User = PrincipalFactory.CreateWithUserId(userId);
        return userId;
    }

    private IPAddress SetupRemoteIpAddress()
    {
        var ipAddress = this.modelFactory.NextIpAddress();
        this.httpContext.Features.Set<IHttpConnectionFeature>(
            new HttpConnectionFeature { RemoteIpAddress = ipAddress });
        return ipAddress;
    }
}
