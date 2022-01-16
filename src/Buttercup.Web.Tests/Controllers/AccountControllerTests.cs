using Buttercup.DataAccess;
using Buttercup.TestUtils;
using Buttercup.Web.Authentication;
using Buttercup.Web.Models;
using Buttercup.Web.TestUtils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using MySqlConnector;
using Xunit;

namespace Buttercup.Web.Controllers;

public class AccountControllerTests
{
    #region Show (GET)

    [Fact]
    public void ShowReturnsViewResultWithCurrentUser()
    {
        using var fixture = new AccountControllerFixture();

        var user = ModelFactory.CreateUser();

        fixture.HttpContext.SetCurrentUser(user);

        var result = fixture.AccountController.Show();
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(user, viewResult.Model);
    }

    #endregion

    #region ChangePassword (GET)

    [Fact]
    public void ChangePasswordGetReturnsViewResult()
    {
        using var fixture = new AccountControllerFixture();

        var result = fixture.AccountController.ChangePassword();
        Assert.IsType<ViewResult>(result);
    }

    #endregion

    #region ChangePassword (POST)

    [Fact]
    public async Task ChangePasswordPostReturnsViewResultWhenModelIsInvalid()
    {
        using var fixture = new ChangePasswordPostFixture();

        fixture.AccountController.ModelState.AddModelError("test", "test");

        var result = await fixture.ChangePasswordPost();

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(fixture.Model, viewResult.Model);
    }

    [Fact]
    public async Task ChangePasswordPostAddsErrorWhenCurrentPasswordIsIncorrect()
    {
        using var fixture = new ChangePasswordPostFixture();

        fixture.SetupChangePassword(false);

        await fixture.ChangePasswordPost();

        var errors = fixture
            .AccountController
            .ModelState[nameof(ChangePasswordViewModel.CurrentPassword)]!
            .Errors;

        var error = Assert.Single(errors);

        Assert.Equal("translated-wrong-password-error", error.ErrorMessage);
    }

    [Fact]
    public async Task ChangePasswordPostReturnsViewResultWhenCurrentPasswordIsIncorrect()
    {
        using var fixture = new ChangePasswordPostFixture();

        fixture.SetupChangePassword(false);

        var result = await fixture.ChangePasswordPost();

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(fixture.Model, viewResult.Model);
    }

    [Fact]
    public async Task ChangePasswordPostRedirectsToYourAccountOnSuccess()
    {
        using var fixture = new ChangePasswordPostFixture();

        fixture.SetupChangePassword(true);

        var result = await fixture.ChangePasswordPost();

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(AccountController.Show), redirectResult.ActionName);
    }

    private class ChangePasswordPostFixture : AccountControllerFixture
    {
        public ChangePasswordPostFixture() =>
            this.MockLocalizer.SetupLocalizedString(
                "Error_WrongPassword", "translated-wrong-password-error");

        public ChangePasswordViewModel Model { get; } = new()
        {
            CurrentPassword = "current-password",
            NewPassword = "new-password",
        };

        public void SetupChangePassword(bool result) =>
            this.MockAuthenticationManager
                .Setup(x => x.ChangePassword(this.HttpContext, "current-password", "new-password"))
                .ReturnsAsync(result);

        public Task<IActionResult> ChangePasswordPost() =>
            this.AccountController.ChangePassword(this.Model);
    }

    #endregion

    #region Preferences (GET)

    [Fact]
    public void PreferencesGetReturnsViewResultWithViewModel()
    {
        using var fixture = new AccountControllerFixture();

        var user = ModelFactory.CreateUser();

        fixture.HttpContext.SetCurrentUser(user);

        var result = fixture.AccountController.Preferences();

        var viewResult = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<PreferencesViewModel>(viewResult.Model);
        Assert.Equal(user.TimeZone, viewModel.TimeZone);
    }

    #endregion

    #region Preferences (POST)

    [Fact]
    public async Task PreferencesPostReturnsViewResultWhenModelIsInvalid()
    {
        using var fixture = new AccountControllerFixture();

        fixture.AccountController.ModelState.AddModelError("test", "test");

        var viewModel = new PreferencesViewModel { TimeZone = "time-zone" };

        var result = await fixture.AccountController.Preferences(viewModel);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(viewModel, viewResult.Model);
    }

    [Fact]
    public async Task PreferencesPostUpdatesUserAndRedirectsToShowPage()
    {
        using var fixture = new AccountControllerFixture();

        var currentUser = ModelFactory.CreateUser();

        fixture.HttpContext.SetCurrentUser(currentUser);

        var viewModel = new PreferencesViewModel { TimeZone = "time-zone" };

        fixture.MockUserDataProvider
            .Setup(x => x.UpdatePreferences(
                fixture.MySqlConnection, currentUser.Id, viewModel.TimeZone))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var result = await fixture.AccountController.Preferences(viewModel);

        fixture.MockUserDataProvider.Verify();

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(AccountController.Show), redirectResult.ActionName);
    }

    #endregion

    private class AccountControllerFixture : IDisposable
    {
        public AccountControllerFixture()
        {
            var mySqlConnectionSource = Mock.Of<IMySqlConnectionSource>(
                x => x.OpenConnection() == Task.FromResult(this.MySqlConnection));

            this.AccountController = new(
                mySqlConnectionSource,
                this.MockUserDataProvider.Object,
                this.MockAuthenticationManager.Object,
                this.MockLocalizer.Object)
            {
                ControllerContext = new()
                {
                    HttpContext = this.HttpContext,
                },
            };
        }

        public AccountController AccountController { get; }

        public DefaultHttpContext HttpContext { get; } = new();

        public MySqlConnection MySqlConnection { get; } = new();

        public Mock<IUserDataProvider> MockUserDataProvider { get; } = new();

        public Mock<IAuthenticationManager> MockAuthenticationManager { get; } = new();

        public Mock<IStringLocalizer<AccountController>> MockLocalizer { get; } = new();

        public void Dispose()
        {
            if (this.AccountController != null)
            {
                this.AccountController.Dispose();
            }
        }
    }
}
