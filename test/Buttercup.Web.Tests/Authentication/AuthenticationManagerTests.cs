using System.Data.Common;
using System.Security.Claims;
using System.Threading.Tasks;
using Buttercup.DataAccess;
using Buttercup.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Buttercup.Web.Authentication
{
    public class AuthenticationManagerTests
    {
        #region Authenticate

        [Fact]
        public async Task AuthenticateReturnsUserOnSuccess()
        {
            var context = new Context();

            var expected = new User
            {
                HashedPassword = "sample-hashed-password",
            };

            context.MockUserDataProvider
                .Setup(x => x.FindUserByEmail(context.MockConnection.Object, "sample@example.com"))
                .ReturnsAsync(expected);

            context.MockPasswordHasher
                .Setup(x => x.VerifyHashedPassword(
                    expected, "sample-hashed-password", "sample-password"))
                .Returns(PasswordVerificationResult.Success);

            var actual = await context.AuthenticationManager.Authenticate(
                "sample@example.com", "sample-password");

            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task AuthenticateReturnsNullWhenEmailIsUnrecognized()
        {
            var context = new Context();

            context.MockUserDataProvider
                .Setup(x => x.FindUserByEmail(context.MockConnection.Object, "sample@example.com"))
                .Returns(Task.FromResult<User>(null));

            Assert.Null(await context.AuthenticationManager.Authenticate(
                "sample@example.com", "sample-password"));
        }

        [Fact]
        public async Task AuthenticateReturnsNullWhenNoPasswordSet()
        {
            var context = new Context();

            context.MockUserDataProvider
                .Setup(x => x.FindUserByEmail(context.MockConnection.Object, "sample@example.com"))
                .ReturnsAsync(new User { HashedPassword = null });

            Assert.Null(await context.AuthenticationManager.Authenticate(
                "sample@example.com", "sample-password"));
        }

        [Fact]
        public async Task AuthenticateReturnsNullWhenPasswordIsIncorrect()
        {
            var context = new Context();

            var user = new User
            {
                HashedPassword = "sample-hashed-password",
            };

            context.MockUserDataProvider
                .Setup(x => x.FindUserByEmail(context.MockConnection.Object, "sample@example.com"))
                .ReturnsAsync(user);

            context.MockPasswordHasher
                .Setup(x => x.VerifyHashedPassword(
                    user, "sample-hashed-password", "sample-password"))
                .Returns(PasswordVerificationResult.Failed);

            Assert.Null(await context.AuthenticationManager.Authenticate(
                "sample@example.com", "sample-password"));
        }

        #endregion

        #region SignIn

        [Fact]
        public async Task SignInSignsInUser()
        {
            var context = new Context();

            var httpContext = new DefaultHttpContext();

            ClaimsPrincipal principal = null;

            context.MockAuthenticationService
                .Setup(x => x.SignInAsync(
                    httpContext,
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    It.IsAny<ClaimsPrincipal>(),
                    null))
                .Callback<HttpContext, string, ClaimsPrincipal, AuthenticationProperties>(
                   (contextArg, scheme, principalArg, properties) => principal = principalArg)
                .Returns(Task.CompletedTask);

            await context.AuthenticationManager.SignIn(
                httpContext, new User { Id = 6, Email = "sample@example.com" });

            Assert.Equal("6", principal.FindFirstValue(ClaimTypes.NameIdentifier));
            Assert.Equal("sample@example.com", principal.Identity.Name);
        }

        #endregion

        private class Context
        {
            public Context()
            {
                this.AuthenticationManager = new AuthenticationManager(
                    this.MockAuthenticationService.Object,
                    this.MockDbConnectionSource.Object,
                    this.MockLogger.Object,
                    this.MockPasswordHasher.Object,
                    this.MockUserDataProvider.Object);

                this.MockDbConnectionSource
                    .Setup(x => x.OpenConnection())
                    .ReturnsAsync(this.MockConnection.Object);
            }

            public AuthenticationManager AuthenticationManager { get; }

            public Mock<IAuthenticationService> MockAuthenticationService { get; } =
                new Mock<IAuthenticationService>();

            public Mock<DbConnection> MockConnection { get; } = new Mock<DbConnection>();

            public Mock<IDbConnectionSource> MockDbConnectionSource { get; } =
                new Mock<IDbConnectionSource>();

            public Mock<ILogger<AuthenticationManager>> MockLogger { get; } =
                new Mock<ILogger<AuthenticationManager>>();

            public Mock<IPasswordHasher<User>> MockPasswordHasher { get; } =
                new Mock<IPasswordHasher<User>>();

            public Mock<IUserDataProvider> MockUserDataProvider { get; } =
                new Mock<IUserDataProvider>();
        }
    }
}
