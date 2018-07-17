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

            context.SetupFindUserByEmail("sample@example.com", expected);

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

            context.SetupFindUserByEmail("sample@example.com", null);

            Assert.Null(await context.AuthenticationManager.Authenticate(
                "sample@example.com", "sample-password"));
        }

        [Fact]
        public async Task AuthenticateReturnsNullWhenNoPasswordSet()
        {
            var context = new Context();

            context.SetupFindUserByEmail("sample@example.com", new User { HashedPassword = null });

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

            context.SetupFindUserByEmail("sample@example.com", user);

            context.MockPasswordHasher
                .Setup(x => x.VerifyHashedPassword(
                    user, "sample-hashed-password", "sample-password"))
                .Returns(PasswordVerificationResult.Failed);

            Assert.Null(await context.AuthenticationManager.Authenticate(
                "sample@example.com", "sample-password"));
        }

        #endregion

        #region PasswordResetTokenIsValid

        [Fact]
        public async Task PasswordResetTokenIsValidDeletesExpiredTokens()
        {
            var context = new Context();

            await context.AuthenticationManager.PasswordResetTokenIsValid("sample-token");

            context.MockPasswordResetTokenDataProvider.Verify(
                x => x.DeleteExpiredTokens(context.MockConnection.Object));
        }

        [Fact]
        public async Task PasswordResetTokenIsValidReturnsTrueIfValid()
        {
            var context = new Context();

            context.SetupGetUserIdForToken("sample-token", 43);

            Assert.True(
                await context.AuthenticationManager.PasswordResetTokenIsValid("sample-token"));
        }

        [Fact]
        public async Task PasswordResetTokenIsValidReturnsFalseIfInvalid()
        {
            var context = new Context();

            context.SetupGetUserIdForToken("sample-token", null);

            Assert.False(
                await context.AuthenticationManager.PasswordResetTokenIsValid("sample-token"));
        }

        #endregion

        #region SendPasswordResetLink

        [Fact]
        public async Task SendPasswordResetLinkInsertsPasswordResetToken()
        {
            var context = new SendPasswordResetLinkContext();

            context.SetupSuccess();

            await context.SendPasswordResetLink();

            context.MockPasswordResetTokenDataProvider.Verify(
                x => x.InsertToken(context.MockConnection.Object, context.User.Id, context.Token));
        }

        [Fact]
        public async Task SendPasswordResetLinkSendsTokenToUser()
        {
            var context = new SendPasswordResetLinkContext();

            context.SetupSuccess();

            await context.SendPasswordResetLink();

            context.MockAuthenticationMailer.Verify(
                x => x.SendPasswordResetLink(context.User.Email, context.Token));
        }

        [Fact]
        public async Task SendPasswordResetLinkDoesNotSendLinkWhenEmailIsUnrecognized()
        {
            var context = new SendPasswordResetLinkContext();

            context.SetupUnrecognizedEmail();

            await context.SendPasswordResetLink();

            context.MockAuthenticationMailer.Verify(
                x => x.SendPasswordResetLink(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
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

        #region SignOut

        [Fact]
        public async Task SignOutSignsOutUser()
        {
            var context = new Context();

            var httpContext = new DefaultHttpContext();

            await context.AuthenticationManager.SignOut(httpContext);

            context.MockAuthenticationService.Verify(x => x.SignOutAsync(httpContext, null, null));
        }

        #endregion

        private class Context
        {
            public Context()
            {
                this.AuthenticationManager = new AuthenticationManager(
                    this.MockAuthenticationMailer.Object,
                    this.MockAuthenticationService.Object,
                    this.MockDbConnectionSource.Object,
                    this.MockLogger.Object,
                    this.MockPasswordHasher.Object,
                    this.MockPasswordResetTokenDataProvider.Object,
                    this.MockRandomTokenGenerator.Object,
                    this.MockUserDataProvider.Object);

                this.MockDbConnectionSource
                    .Setup(x => x.OpenConnection())
                    .ReturnsAsync(this.MockConnection.Object);
            }

            public AuthenticationManager AuthenticationManager { get; }

            public Mock<IAuthenticationMailer> MockAuthenticationMailer { get; } =
                new Mock<IAuthenticationMailer>();

            public Mock<IAuthenticationService> MockAuthenticationService { get; } =
                new Mock<IAuthenticationService>();

            public Mock<DbConnection> MockConnection { get; } = new Mock<DbConnection>();

            public Mock<IDbConnectionSource> MockDbConnectionSource { get; } =
                new Mock<IDbConnectionSource>();

            public Mock<ILogger<AuthenticationManager>> MockLogger { get; } =
                new Mock<ILogger<AuthenticationManager>>();

            public Mock<IPasswordHasher<User>> MockPasswordHasher { get; } =
                new Mock<IPasswordHasher<User>>();

            public Mock<IPasswordResetTokenDataProvider> MockPasswordResetTokenDataProvider { get; } =
                new Mock<IPasswordResetTokenDataProvider>();

            public Mock<IRandomTokenGenerator> MockRandomTokenGenerator { get; } =
                new Mock<IRandomTokenGenerator>();

            public Mock<IUserDataProvider> MockUserDataProvider { get; } =
                new Mock<IUserDataProvider>();

            public void SetupFindUserByEmail(string email, User user) =>
                this.MockUserDataProvider
                    .Setup(x => x.FindUserByEmail(this.MockConnection.Object, email))
                    .ReturnsAsync(user);

            public void SetupGetUserIdForToken(string token, long? userId) =>
                this.MockPasswordResetTokenDataProvider
                    .Setup(x => x.GetUserIdForToken(this.MockConnection.Object, token))
                    .ReturnsAsync(userId);
        }

        private class SendPasswordResetLinkContext : Context
        {
            public string Email { get; } = "sample@example.com";

            public User User { get; private set; }

            public string Token { get; private set; }

            public void SetupSuccess()
            {
                this.User = new User { Id = 31, Email = "sample-x@example.com" };

                this.SetupFindUserByEmail(this.Email, this.User);

                this.MockRandomTokenGenerator
                    .Setup(x => x.Generate())
                    .Returns(this.Token = "sample-token");
            }

            public void SetupUnrecognizedEmail() =>
                this.SetupFindUserByEmail(this.Email, this.User = null);

            public Task SendPasswordResetLink() =>
                this.AuthenticationManager.SendPasswordResetLink(this.Email);
        }
    }
}
