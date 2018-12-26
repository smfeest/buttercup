using System;
using Buttercup.Models;
using Buttercup.Web.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Buttercup.Web.Controllers
{
    public class AccountControllerTests
    {
        #region Show (GET)

        [Fact]
        public void ShowReturnsViewResultWithCurrentUser()
        {
            using (var context = new Context())
            {
                var user = new User();

                context.HttpContext.SetCurrentUser(user);

                var result = context.AccountController.Show();
                var viewResult = Assert.IsType<ViewResult>(result);
                Assert.Same(user, viewResult.Model);
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

                this.AccountController = new AccountController()
                {
                    ControllerContext = this.ControllerContext,
                };
            }

            public AccountController AccountController { get; }

            public ControllerContext ControllerContext { get; }

            public DefaultHttpContext HttpContext { get; } = new DefaultHttpContext();

            public void Dispose()
            {
                if (this.AccountController != null)
                {
                    this.AccountController.Dispose();
                }
            }
        }
    }
}
