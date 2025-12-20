using System.Net;
using Buttercup.Application;
using Buttercup.EntityModel;
using Buttercup.Security;
using Buttercup.TestUtils;
using Buttercup.Web.Models.Account;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Buttercup.Web.Controllers;

public sealed class AccountControllerTests : IDisposable
{
    private readonly ModelFactory modelFactory = new();

    private readonly DefaultHttpContext httpContext = new();
    private readonly Mock<ICookieAuthenticationService> cookieAuthenticationServiceMock = new();
    private readonly DictionaryLocalizer<AccountController> localizer = new();
    private readonly Mock<IPasswordAuthenticationService> passwordAuthenticationServiceMock = new();
    private readonly Mock<IUserManager> userManagerMock = new();

    private readonly AccountController accountController;

    public AccountControllerTests() =>
        this.accountController = new(
            this.userManagerMock.Object,
            this.cookieAuthenticationServiceMock.Object,
            this.passwordAuthenticationServiceMock.Object,
            this.localizer)
        {
            ControllerContext = new() { HttpContext = this.httpContext },
        };

    public void Dispose() => this.accountController.Dispose();

    #region Show (GET)

    [Fact]
    public async Task Show_ReturnsViewResultWithCurrentUser()
    {
        var user = this.SetupCurrentUser();
        this.SetupFindUser(user.Id, user);

        var result = await this.accountController.Show();
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(user, viewResult.Model);
    }

    [Fact]
    public async Task Show_UserNotFound_ReturnsNotFoundResult()
    {
        var userId = this.SetupCurrentUserId();
        this.SetupFindUser(userId, null);

        var result = await this.accountController.Show();
        Assert.IsType<NotFoundResult>(result);
    }

    #endregion

    #region ChangePassword (GET)

    [Fact]
    public void ChangePassword_Get_ReturnsViewResult()
    {
        var result = this.accountController.ChangePassword();
        Assert.IsType<ViewResult>(result);
    }

    #endregion

    #region ChangePassword (POST)

    [Fact]
    public async Task ChangePassword_Post_ModelIsInvalid_ReturnsViewResult()
    {
        this.accountController.ModelState.AddModelError("test", "test");

        var viewModel = BuildChangePasswordViewModel();
        var result = await this.accountController.ChangePassword(viewModel);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(viewModel, viewResult.Model);
    }

    [Fact]
    public async Task ChangePassword_Post_CurrentPasswordIsIncorrect_AddsError()
    {
        this.SetupChangePassword(false);

        await this.accountController.ChangePassword(BuildChangePasswordViewModel());

        var modelState =
            this.accountController.ModelState[nameof(ChangePasswordViewModel.CurrentPassword)];
        Assert.NotNull(modelState);

        var error = Assert.Single(modelState.Errors);
        Assert.Equal("translated-wrong-password-error", error.ErrorMessage);
    }

    [Fact]
    public async Task ChangePassword_Post_CurrentPasswordIsIncorrect_ReturnsViewResult()
    {
        this.SetupChangePassword(false);

        var viewModel = BuildChangePasswordViewModel();
        var result = await this.accountController.ChangePassword(viewModel);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(viewModel, viewResult.Model);
    }

    [Fact]
    public async Task ChangePassword_Post_Success_RefreshesPrincipal()
    {
        this.SetupChangePassword(true);

        await this.accountController.ChangePassword(BuildChangePasswordViewModel());

        this.cookieAuthenticationServiceMock.Verify(x => x.RefreshPrincipal(this.httpContext));
    }

    [Fact]
    public async Task ChangePassword_Post_Success_RedirectsToYourAccount()
    {
        this.SetupChangePassword(true);

        var result = await this.accountController.ChangePassword(BuildChangePasswordViewModel());

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(AccountController.Show), redirectResult.ActionName);
    }

    private static ChangePasswordViewModel BuildChangePasswordViewModel() => new()
    {
        CurrentPassword = "current-password",
        NewPassword = "new-password",
    };

    private void SetupChangePassword(bool result)
    {
        var userId = this.SetupCurrentUserId();
        var ipAddress = this.SetupRemoteIpAddress();

        this.passwordAuthenticationServiceMock
            .Setup(x => x.ChangePassword(userId, "current-password", "new-password", ipAddress))
            .ReturnsAsync(result);

        this.localizer.Add("Error_WrongPassword", "translated-wrong-password-error");
    }

    #endregion

    #region Preferences (GET)

    [Fact]
    public async Task Preferences_Get_ReturnsViewResultWithViewModel()
    {
        var user = this.SetupCurrentUser();
        this.SetupFindUser(user.Id, user);

        var result = await this.accountController.Preferences();

        var viewResult = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<PreferencesViewModel>(viewResult.Model);
        Assert.Equal(user.TimeZone, viewModel.TimeZone);
    }

    [Fact]
    public async Task Preferences_Get_UserNotFound_ReturnsNotFoundResult()
    {
        var userId = this.SetupCurrentUserId();
        this.SetupFindUser(userId, null);

        var result = await this.accountController.Preferences();
        Assert.IsType<NotFoundResult>(result);
    }

    #endregion

    #region Preferences (POST)

    [Fact]
    public async Task Preferences_Post_InvalidModel_ReturnsViewResult()
    {
        this.accountController.ModelState.AddModelError("test", "test");

        var viewModel = BuildPreferencesViewModel();
        var result = await this.accountController.Preferences(viewModel);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(viewModel, viewResult.Model);
    }

    [Fact]
    public async Task Preferences_Post_Success_UpdatesUser()
    {
        var userId = this.SetupCurrentUserId();

        await this.accountController.Preferences(BuildPreferencesViewModel());

        this.userManagerMock.Verify(x => x.SetTimeZone(userId, "time-zone"));
    }

    [Fact]
    public async Task Preferences_Post_Success_RedirectsToShowPage()
    {
        this.SetupCurrentUserId();

        var result = await this.accountController.Preferences(BuildPreferencesViewModel());

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(AccountController.Show), redirectResult.ActionName);
    }

    private static PreferencesViewModel BuildPreferencesViewModel() =>
        new() { TimeZone = "time-zone" };

    #endregion

    private User SetupCurrentUser()
    {
        var user = this.modelFactory.BuildUser();
        this.httpContext.User = PrincipalFactory.CreateWithUserId(user.Id);
        return user;
    }

    private long SetupCurrentUserId()
    {
        var userId = this.modelFactory.NextInt();
        this.httpContext.User = PrincipalFactory.CreateWithUserId(userId);
        return userId;
    }

    private void SetupFindUser(long id, User? user) =>
        this.userManagerMock.Setup(x => x.FindUser(id)).ReturnsAsync(user);

    private IPAddress SetupRemoteIpAddress()
    {
        var ipAddress = this.modelFactory.NextIpAddress();
        this.httpContext.Features.Set<IHttpConnectionFeature>(
            new HttpConnectionFeature { RemoteIpAddress = ipAddress });
        return ipAddress;
    }
}
