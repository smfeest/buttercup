using System;
using System.Threading.Tasks;
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
    public class AuthenticationControllerTests
    {
        #region RequestPasswordReset (GET)

        [Fact]
        public void RequestPasswordResetGetReturnsViewResult()
        {
            using (var context = new Context())
            {
                var result = context.AuthenticationController.RequestPasswordReset();
                Assert.IsType<ViewResult>(result);
            }
        }

        #endregion

        #region RequestPasswordReset (POST)

        [Fact]
        public async Task RequestPasswordResetPostReturnsViewResultWhenModelIsInvalid()
        {
            using (var context = new Context())
            {
                context.AuthenticationController.ModelState.AddModelError("test", "test");

                var model = new RequestPasswordResetViewModel();
                var result = await context.AuthenticationController.RequestPasswordReset(model);

                var viewResult = Assert.IsType<ViewResult>(result);
                Assert.Same(model, viewResult.Model);
            }
        }

        [Fact]
        public async Task RequestPasswordResetPostSendsPasswordResetLink()
        {
            using (var context = new Context())
            {
                var model = new RequestPasswordResetViewModel { Email = "sample-user@example.com" };
                var result = await context.AuthenticationController.RequestPasswordReset(model);

                context.MockAuthenticationManager.Verify(x => x.SendPasswordResetLink(
                    context.ControllerContext, "sample-user@example.com"));
            }
        }

        [Fact]
        public async Task RequestPasswordResetPostReturnsViewResult()
        {
            using (var context = new Context())
            {
                var model = new RequestPasswordResetViewModel();

                var result = await context.AuthenticationController.RequestPasswordReset(model);

                var viewResult = Assert.IsType<ViewResult>(result);
                Assert.Equal("RequestPasswordResetConfirmation", viewResult.ViewName);
                Assert.Same(model, viewResult.Model);
            }
        }

        #endregion

        #region ResetPassword (GET)

        [Fact]
        public async Task ResetPasswordGetReturnsDefaultViewResultWhenTokenIsValid()
        {
            using (var context = new Context())
            {
                context.MockAuthenticationManager
                    .Setup(x => x.PasswordResetTokenIsValid("sample-token"))
                    .ReturnsAsync(true);

                var result = await context.AuthenticationController.ResetPassword("sample-token");

                var viewResult = Assert.IsType<ViewResult>(result);
                Assert.Null(viewResult.ViewName);
            }
        }

        [Fact]
        public async Task ResetPasswordGetReturnsInvalidTokenViewResultWhenTokenIsInvalid()
        {
            using (var context = new Context())
            {
                context.MockAuthenticationManager
                    .Setup(x => x.PasswordResetTokenIsValid("sample-token"))
                    .ReturnsAsync(false);

                var result = await context.AuthenticationController.ResetPassword("sample-token");

                var viewResult = Assert.IsType<ViewResult>(result);
                Assert.Equal("ResetPasswordInvalidToken", viewResult.ViewName);
            }
        }

        #endregion

        #region ResetPassword (POST)

        [Fact]
        public async Task ResetPasswordPostReturnsDefaultViewResultWhenModelIsInvalid()
        {
            using (var context = new Context())
            {
                context.AuthenticationController.ModelState.AddModelError("test", "test");

                var viewModel = new ResetPasswordViewModel();
                var result = await context.AuthenticationController.ResetPassword(
                    "sample-token", viewModel);

                var viewResult = Assert.IsType<ViewResult>(result);
                Assert.Same(viewModel, viewResult.Model);
            }
        }

        [Fact]
        public async Task ResetPasswordPostReturnsInvalidTokenViewResultWhenTokenIsInvalid()
        {
            using (var context = new Context())
            {
                context.MockAuthenticationManager
                    .Setup(x => x.ResetPassword("sample-token", "sample-password"))
                    .ThrowsAsync(new InvalidTokenException());

                var result = await context.AuthenticationController.ResetPassword(
                    "sample-token", new ResetPasswordViewModel { Password = "sample-password" });

                var viewResult = Assert.IsType<ViewResult>(result);
                Assert.Equal("ResetPasswordInvalidToken", viewResult.ViewName);
            }
        }

        [Fact]
        public async Task ResetPasswordPostSignsInUser()
        {
            using (var context = new Context())
            {
                var user = new User();

                context.MockAuthenticationManager
                    .Setup(x => x.ResetPassword("sample-token", "sample-password"))
                    .ReturnsAsync(user);

                await context.AuthenticationController.ResetPassword(
                    "sample-token", new ResetPasswordViewModel { Password = "sample-password" });

                context.MockAuthenticationManager.Verify(x => x.SignIn(context.HttpContext, user));
            }
        }

        [Fact]
        public async Task ResetPasswordPostRedirectsToHomeIndex()
        {
            using (var context = new Context())
            {
                var result = await context.AuthenticationController.ResetPassword(
                    "sample-token", new ResetPasswordViewModel());

                var redirectResult = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal("Home", redirectResult.ControllerName);
                Assert.Equal(nameof(HomeController.Index), redirectResult.ActionName);
            }
        }

        #endregion

        #region SignIn (GET)

        [Fact]
        public void SignInGetReturnsViewResult()
        {
            using (var context = new Context())
            {
                var result = context.AuthenticationController.SignIn();
                Assert.IsType<ViewResult>(result);
            }
        }

        #endregion

        #region SignIn (POST)

        [Fact]
        public async Task SignInPostReturnsViewResultWhenModelIsInvalid()
        {
            using (var context = new SignInContext())
            {
                context.AuthenticationController.ModelState.AddModelError("test", "test");

                var result = await context.SignInPost();

                var viewResult = Assert.IsType<ViewResult>(result);
                Assert.Same(context.Model, viewResult.Model);
            }
        }

        [Fact]
        public async Task SignInPostAddsErrorWhenAuthenticationFails()
        {
            using (var context = new SignInContext())
            {
                context.SetupAuthenticate(null);

                await context.SignInPost();

                var error = Assert.Single(
                    context.AuthenticationController.ModelState[string.Empty].Errors);

                Assert.Equal("translated-wrong-email-or-password-error", error.ErrorMessage);
            }
        }

        [Fact]
        public async Task SignInPostReturnsViewResultWhenAuthenticationFails()
        {
            using (var context = new SignInContext())
            {
                context.SetupAuthenticate(null);

                var result = await context.SignInPost();

                var viewResult = Assert.IsType<ViewResult>(result);
                Assert.Same(context.Model, viewResult.Model);
            }
        }

        [Fact]
        public async Task SignInSignsInUserWhenSuccessful()
        {
            using (var context = new SignInContext())
            {
                var user = new User();

                context.SetupAuthenticate(user);

                await context.SignInPost();

                context.MockAuthenticationManager.Verify(x => x.SignIn(context.HttpContext, user));
            }
        }

        [Fact]
        public async Task SignInPostRedirectsToInternalUrl()
        {
            using (var context = new SignInContext())
            {
                context.SetupAuthenticate(new User());

                context.MockUrlHelper.Setup(x => x.IsLocalUrl("/sample/redirect")).Returns(true);

                var result = await context.SignInPost("/sample/redirect");

                var redirectResult = Assert.IsType<RedirectResult>(result);
                Assert.Equal("/sample/redirect", redirectResult.Url);
            }
        }

        [Fact]
        public async Task SignInPostRedirectsDoesNotRedirectToExternalUrl()
        {
            using (var context = new SignInContext())
            {
                context.SetupAuthenticate(new User());

                context.MockUrlHelper.Setup(x => x.IsLocalUrl("https://evil.com/")).Returns(false);

                var result = await context.SignInPost("https://evil.com/");

                var redirectResult = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal("Home", redirectResult.ControllerName);
                Assert.Equal(nameof(HomeController.Index), redirectResult.ActionName);
            }
        }

        #endregion

        #region SignOut

        [Fact]
        public void SignOutSignsOutUser()
        {
            using (var context = new Context())
            {
                var result = context.AuthenticationController.SignOut();

                context.MockAuthenticationManager.Verify(x => x.SignOut(context.HttpContext));
            }
        }

        [Fact]
        public void SignOutSetsCacheControlHeader()
        {
            using (var context = new Context())
            {
                var result = context.AuthenticationController.SignOut();

                var cacheControlHeader = context.HttpContext.Response.GetTypedHeaders().CacheControl;

                Assert.True(cacheControlHeader.NoCache);
                Assert.True(cacheControlHeader.NoStore);
            }
        }

        [Fact]
        public async Task SignOutRedirectsToInternalUrls()
        {
            using (var context = new Context())
            {
                context.MockUrlHelper.Setup(x => x.IsLocalUrl("/sample/redirect")).Returns(true);

                var result = await context.AuthenticationController.SignOut("/sample/redirect");

                var redirectResult = Assert.IsType<RedirectResult>(result);
                Assert.Equal("/sample/redirect", redirectResult.Url);
            }
        }

        [Fact]
        public async Task SignOutDoesNotRedirectToExternalUrls()
        {
            using (var context = new Context())
            {
                context.MockUrlHelper.Setup(x => x.IsLocalUrl("https://evil.com/")).Returns(false);

                var result = await context.AuthenticationController.SignOut("https://evil.com/");

                var redirectResult = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal("Home", redirectResult.ControllerName);
                Assert.Equal(nameof(HomeController.Index), redirectResult.ActionName);
            }
        }

        #endregion

        private class Context : IDisposable
        {
            public Context()
            {
                this.ControllerContext = new ControllerContext()
                {
                    HttpContext = this.HttpContext,
                };

                this.AuthenticationController = new AuthenticationController(
                    this.MockAuthenticationManager.Object,
                    this.MockLocalizer.Object)
                {
                    ControllerContext = this.ControllerContext,
                    Url = this.MockUrlHelper.Object,
                };
            }

            public AuthenticationController AuthenticationController { get; }

            public ControllerContext ControllerContext { get; }

            public DefaultHttpContext HttpContext { get; } = new DefaultHttpContext();

            public Mock<IAuthenticationManager> MockAuthenticationManager { get; } =
                new Mock<IAuthenticationManager>();

            public Mock<IStringLocalizer<AuthenticationController>> MockLocalizer { get; } =
                new Mock<IStringLocalizer<AuthenticationController>>();

            public Mock<IUrlHelper> MockUrlHelper { get; } = new Mock<IUrlHelper>();

            public void Dispose()
            {
                if (this.AuthenticationController != null)
                {
                    this.AuthenticationController.Dispose();
                }
            }
        }

        private class SignInContext : Context
        {
            public SignInContext() =>
                this.MockLocalizer
                    .SetupGet(x => x["Error_WrongEmailOrPassword"])
                    .Returns(new LocalizedString(
                        "Error_WrongPassword", "translated-wrong-email-or-password-error"));

            public SignInViewModel Model { get; } = new SignInViewModel
            {
                Email = "sample@example.com",
                Password = "sample-password",
            };

            public void SetupAuthenticate(User user)
            {
                this.MockAuthenticationManager
                    .Setup(x => x.Authenticate("sample@example.com", "sample-password"))
                    .ReturnsAsync(user);
            }

            public Task<IActionResult> SignInPost(string returnUrl = null) =>
                this.AuthenticationController.SignIn(this.Model, returnUrl);
        }
    }
}
