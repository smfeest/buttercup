using System.Net;
using Buttercup.DataAccess;
using Buttercup.EntityModel;
using Buttercup.Security;
using Buttercup.TestUtils;
using Buttercup.Web.Models;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using Xunit;

namespace Buttercup.Web.Controllers;

public sealed class AccountControllerTests : IDisposable
{
    private readonly ModelFactory modelFactory = new();

    private readonly FakeDbContextFactory dbContextFactory = new();
    private readonly DefaultHttpContext httpContext = new();
    private readonly Mock<ICookieAuthenticationService> cookieAuthenticationServiceMock = new();
    private readonly Mock<IStringLocalizer<AccountController>> localizerMock = new();
    private readonly Mock<IPasswordAuthenticationService> passwordAuthenticationServiceMock = new();
    private readonly Mock<IUserDataProvider> userDataProviderMock = new();

    private readonly AccountController accountController;

    public AccountControllerTests() =>
        this.accountController = new(
            this.dbContextFactory,
            this.userDataProviderMock.Object,
            this.cookieAuthenticationServiceMock.Object,
            this.passwordAuthenticationServiceMock.Object,
            this.localizerMock.Object)
        {
            ControllerContext = new() { HttpContext = this.httpContext },
        };

    public void Dispose()
    {
        this.accountController.Dispose();
        this.dbContextFactory.Dispose();
    }

    #region Show (GET)

    [Fact]
    public async void Show_ReturnsViewResultWithCurrentUser()
    {
        var user = this.SetupCurrentUser();

        var result = await this.accountController.Show();
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(user, viewResult.Model);
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

        this.localizerMock.SetupLocalizedString(
            "Error_WrongPassword", "translated-wrong-password-error");
    }

    #endregion

    #region Preferences (GET)

    [Fact]
    public async void Preferences_Get_ReturnsViewResultWithViewModel()
    {
        var user = this.SetupCurrentUser();

        var result = await this.accountController.Preferences();

        var viewResult = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<PreferencesViewModel>(viewResult.Model);
        Assert.Equal(user.TimeZone, viewModel.TimeZone);
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

        this.userDataProviderMock
            .Verify(x => x.UpdatePreferences(
                this.dbContextFactory.FakeDbContext, userId, "time-zone"));
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

        this.userDataProviderMock
            .Setup(x => x.GetUser(this.dbContextFactory.FakeDbContext, user.Id))
            .ReturnsAsync(user);

        return user;
    }

    private long SetupCurrentUserId()
    {
        var userId = this.modelFactory.NextInt();
        this.httpContext.User = PrincipalFactory.CreateWithUserId(userId);
        return userId;
    }

    private IPAddress SetupRemoteIpAddress()
    {
        var ipAddress = new IPAddress(this.modelFactory.NextInt());
        this.httpContext.Features.Set<IHttpConnectionFeature>(
            new HttpConnectionFeature { RemoteIpAddress = ipAddress });
        return ipAddress;
    }
}
