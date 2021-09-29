using System;
using System.Data.Common;
using System.Threading.Tasks;
using Buttercup.DataAccess;
using Buttercup.Models;
using Buttercup.Web.Authentication;
using Buttercup.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using Xunit;

namespace Buttercup.Web.Controllers
{
    public class AccountControllerTests
    {
        #region Show (GET)

        [Fact]
        public void ShowReturnsViewResultWithCurrentUser()
        {
            using var context = new Context();

            var user = new User();

            context.HttpContext.SetCurrentUser(user);

            var result = context.AccountController.Show();
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Same(user, viewResult.Model);
        }

        #endregion

        #region ChangePassword (GET)

        [Fact]
        public void ChangePasswordGetReturnsViewResult()
        {
            using var context = new Context();

            var result = context.AccountController.ChangePassword();
            Assert.IsType<ViewResult>(result);
        }

        #endregion

        #region ChangePassword (POST)

        [Fact]
        public async Task ChangePasswordPostReturnsViewResultWhenModelIsInvalid()
        {
            using var context = new ChangePasswordContext();

            context.AccountController.ModelState.AddModelError("test", "test");

            var result = await context.ChangePasswordPost();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Same(context.Model, viewResult.Model);
        }

        [Fact]
        public async Task ChangePasswordPostAddsErrorWhenCurrentPasswordIsIncorrect()
        {
            using var context = new ChangePasswordContext();

            context.SetupChangePassword(false);

            await context.ChangePasswordPost();

            var errors = context
                .AccountController
                .ModelState[nameof(ChangePasswordViewModel.CurrentPassword)]
                .Errors;

            var error = Assert.Single(errors);

            Assert.Equal("translated-wrong-password-error", error.ErrorMessage);
        }

        [Fact]
        public async Task ChangePasswordPostReturnsViewResultWhenCurrentPasswordIsIncorrect()
        {
            using var context = new ChangePasswordContext();

            context.SetupChangePassword(false);

            var result = await context.ChangePasswordPost();

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Same(context.Model, viewResult.Model);
        }

        [Fact]
        public async Task ChangePasswordPostRedirectsToYourAccountOnSuccess()
        {
            using var context = new ChangePasswordContext();

            context.SetupChangePassword(true);

            var result = await context.ChangePasswordPost();

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(AccountController.Show), redirectResult.ActionName);
        }

        #endregion

        #region Preferences (GET)

        [Fact]
        public void PreferencesGetReturnsViewResultWithViewModel()
        {
            using var context = new Context();

            var user = new User { TimeZone = "time-zone" };

            context.HttpContext.SetCurrentUser(user);

            var result = context.AccountController.Preferences();

            var viewResult = Assert.IsType<ViewResult>(result);
            var viewModel = Assert.IsType<PreferencesViewModel>(viewResult.Model);
            Assert.Equal(user.TimeZone, viewModel.TimeZone);
        }

        #endregion

        #region Preferences (POST)

        [Fact]
        public async Task PreferencesPostUpdatesUserAndRedirectsToShowPage()
        {
            using var context = new Context();

            context.HttpContext.SetCurrentUser(new() { Id = 21 });

            var viewModel = new PreferencesViewModel { TimeZone = "time-zone" };

            context.MockUserDataProvider
                .Setup(x => x.UpdatePreferences(
                    context.DbConnection, 21, viewModel.TimeZone, context.UtcNow))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var result = await context.AccountController.Preferences(viewModel);

            context.MockUserDataProvider.Verify();

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(AccountController.Show), redirectResult.ActionName);
        }

        #endregion

        private class Context : IDisposable
        {
            public Context()
            {
                var clock = Mock.Of<IClock>(x => x.UtcNow == this.UtcNow);
                var dbConnectionSource = Mock.Of<IDbConnectionSource>(
                    x => x.OpenConnection() == Task.FromResult(this.DbConnection));

                this.AccountController = new(
                    clock,
                    dbConnectionSource,
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

            public DbConnection DbConnection { get; } = Mock.Of<DbConnection>();

            public Mock<IUserDataProvider> MockUserDataProvider { get; } = new();

            public Mock<IAuthenticationManager> MockAuthenticationManager { get; } = new();

            public Mock<IStringLocalizer<AccountController>> MockLocalizer { get; } = new();

            public DateTime UtcNow { get; } = new(2000, 1, 2, 3, 4, 5, DateTimeKind.Utc);

            public void Dispose()
            {
                if (this.AccountController != null)
                {
                    this.AccountController.Dispose();
                }
            }
        }

        private class ChangePasswordContext : Context
        {
            public ChangePasswordContext()
            {
                this.MockLocalizer
                    .SetupGet(x => x["Error_WrongPassword"])
                    .Returns(new LocalizedString(
                        "Error_WrongPassword", "translated-wrong-password-error"));
            }

            public ChangePasswordViewModel Model { get; } = new()
            {
                CurrentPassword = "current-password",
                NewPassword = "new-password",
            };

            public void SetupChangePassword(bool result) =>
                this.MockAuthenticationManager
                    .Setup(x => x.ChangePassword(
                        this.HttpContext, "current-password", "new-password"))
                    .ReturnsAsync(result);

            public Task<IActionResult> ChangePasswordPost() =>
                this.AccountController.ChangePassword(this.Model);
        }
    }
}
