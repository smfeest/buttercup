using System;
using System.Threading.Tasks;
using Buttercup.Models;
using Buttercup.Web.Authentication;
using Buttercup.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Buttercup.Web.Controllers
{
    public class AuthenticationControllerTests
    {
        #region SignIn (GET)

        [Fact]
        public async Task SignInGetSignsOutCurrentUser()
        {
            using (var context = new Context())
            {
                await context.AuthenticationController.SignIn();

                context.MockAuthenticationManager.Verify(x => x.SignOut(context.HttpContext));
            }
        }

        [Fact]
        public async Task SignInGetReturnsViewResult()
        {
            using (var context = new Context())
            {
                var result = await context.AuthenticationController.SignIn();
                Assert.IsType<ViewResult>(result);
            }
        }

        #endregion

        #region SignIn (POST)

        [Fact]
        public async Task SignInPostReturnsViewResultWhenModelIsInvalid()
        {
            using (var context = new Context())
            {
                context.AuthenticationController.ModelState.AddModelError("test", "test");

                var viewModel = new SignInViewModel();
                var result = await context.AuthenticationController.SignIn(viewModel);

                var viewResult = Assert.IsType<ViewResult>(result);
                Assert.Same(viewModel, viewResult.Model);
            }
        }

        [Fact]
        public async Task SignInPostAddsErrorWhenAuthenticationFails()
        {
            using (var context = new Context())
            {
                context.SetupAuthenticate(null);

                await context.AuthenticationController.SignIn(
                    context.CreateSampleSignInViewModel());

                var error = Assert.Single(
                    context.AuthenticationController.ModelState[string.Empty].Errors);

                Assert.Equal("Wrong email address or password", error.ErrorMessage);
            }
        }

        [Fact]
        public async Task SignInPostReturnsViewResultWhenAuthenticationFails()
        {
            using (var context = new Context())
            {
                context.SetupAuthenticate(null);

                var viewModel = context.CreateSampleSignInViewModel();

                var result = await context.AuthenticationController.SignIn(viewModel);

                var viewResult = Assert.IsType<ViewResult>(result);
                Assert.Same(viewModel, viewResult.Model);
            }
        }

        [Fact]
        public async Task SignInSignsInUserWhenSuccessful()
        {
            using (var context = new Context())
            {
                var user = new User();

                context.SetupAuthenticate(user);

                await context.AuthenticationController.SignIn(
                    context.CreateSampleSignInViewModel());

                context.MockAuthenticationManager.Verify(x => x.SignIn(context.HttpContext, user));
            }
        }

        [Fact]
        public async Task SignInPostRedirectsToInternalUrl()
        {
            using (var context = new Context())
            {
                context.SetupAuthenticate(new User());

                context.MockUrlHelper.Setup(x => x.IsLocalUrl("/sample/redirect")).Returns(true);

                var result = await context.AuthenticationController.SignIn(
                    context.CreateSampleSignInViewModel(), "/sample/redirect");

                var redirectResult = Assert.IsType<RedirectResult>(result);
                Assert.Equal("/sample/redirect", redirectResult.Url);
            }
        }

        [Fact]
        public async Task SignInPostRedirectsDoesNotRedirectToExternalUrl()
        {
            using (var context = new Context())
            {
                context.SetupAuthenticate(new User());

                context.MockUrlHelper.Setup(x => x.IsLocalUrl("https://evil.com/")).Returns(false);

                var result = await context.AuthenticationController.SignIn(
                    context.CreateSampleSignInViewModel(), "https://evil.com/");

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
                this.AuthenticationController = new AuthenticationController(
                    this.MockAuthenticationManager.Object)
                {
                    ControllerContext = new ControllerContext()
                    {
                        HttpContext = this.HttpContext,
                    },
                    Url = this.MockUrlHelper.Object,
                };
            }

            public AuthenticationController AuthenticationController { get; }

            public DefaultHttpContext HttpContext { get; } = new DefaultHttpContext();

            public Mock<IAuthenticationManager> MockAuthenticationManager { get; } =
                new Mock<IAuthenticationManager>();

            public Mock<IUrlHelper> MockUrlHelper { get; } = new Mock<IUrlHelper>();

            public void Dispose()
            {
                if (this.AuthenticationController != null)
                {
                    this.AuthenticationController.Dispose();
                }
            }

            public SignInViewModel CreateSampleSignInViewModel() => new SignInViewModel
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
        }
    }
}
