using System.Data.Common;
using System.Security.Claims;
using System.Threading.Tasks;
using Buttercup.DataAccess;
using Buttercup.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
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

        #region ResetPassword

        [Fact]
        public async Task ResetPasswordDeletesExpiredPasswordResetTokens()
        {
            var context = new ResetPasswordContext();

            context.SetupSuccess();

            await context.ResetPassword();

            context.MockPasswordResetTokenDataProvider.Verify(
                x => x.DeleteExpiredTokens(context.MockConnection.Object));
        }

        [Fact]
        public async Task ResetPasswordThrowsIfTokenIsInvalid()
        {
            var context = new ResetPasswordContext();

            context.SetupInvalidToken();

            await Assert.ThrowsAsync<InvalidTokenException>(context.ResetPassword);
        }

        [Fact]
        public async Task ResetPasswordUpdatesPassword()
        {
            var context = new ResetPasswordContext();

            context.SetupSuccess();

            context.MockPasswordHasher
                .Setup(x => x.HashPassword(null, context.NewPassword))
                .Returns("sample-hashed-password");

            await context.ResetPassword();

            context.MockUserDataProvider.Verify(x => x.UpdatePassword(
                context.MockConnection.Object, context.UserId.Value, "sample-hashed-password"));
        }

        [Fact]
        public async Task ResetPasswordDeletesPasswordResetTokens()
        {
            var context = new ResetPasswordContext();

            context.SetupSuccess();

            await context.ResetPassword();

            context.MockPasswordResetTokenDataProvider.Verify(
                x => x.DeleteTokensForUser(context.MockConnection.Object, context.UserId.Value));
        }

        [Fact]
        public async Task ResetPasswordReturnsUser()
        {
            var context = new ResetPasswordContext();

            context.SetupSuccess();

            var actual = await context.ResetPassword();

            Assert.Equal(context.User, actual);
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
        public async Task SendPasswordResetLinkSendsLinkToUser()
        {
            var context = new SendPasswordResetLinkContext();

            context.SetupSuccess();

            await context.SendPasswordResetLink();

            context.MockAuthenticationMailer.Verify(
                x => x.SendPasswordResetLink(context.User.Email, context.Link));
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
                    this.MockUrlHelperFactory.Object,
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

            public Mock<IUrlHelperFactory> MockUrlHelperFactory { get; } =
                new Mock<IUrlHelperFactory>();

            public Mock<IUserDataProvider> MockUserDataProvider { get; } =
                new Mock<IUserDataProvider>();

            public void SetupFindUserByEmail(string email, User user) =>
                this.MockUserDataProvider
                    .Setup(x => x.FindUserByEmail(this.MockConnection.Object, email))
                    .ReturnsAsync(user);

            public void SetupGetUser(long id, User user) =>
                this.MockUserDataProvider
                    .Setup(x => x.GetUser(this.MockConnection.Object, id))
                    .ReturnsAsync(user);

            public void SetupGetUserIdForToken(string token, long? userId) =>
                this.MockPasswordResetTokenDataProvider
                    .Setup(x => x.GetUserIdForToken(this.MockConnection.Object, token))
                    .ReturnsAsync(userId);
        }

        private class ResetPasswordContext : Context
        {
            public string Token { get; } = "sample-token";

            public string NewPassword { get; } = "sample-password";

            public long? UserId { get; private set; }

            public User User { get; private set; }

            public void SetupInvalidToken() =>
                this.SetupGetUserIdForToken(this.Token, this.UserId = null);

            public void SetupSuccess()
            {
                this.SetupGetUserIdForToken(this.Token, this.UserId = 23);
                this.SetupGetUser(this.UserId.Value, this.User = new User());
            }

            public Task<User> ResetPassword() =>
                this.AuthenticationManager.ResetPassword(this.Token, this.NewPassword);
        }

        private class SendPasswordResetLinkContext : Context
        {
            public SendPasswordResetLinkContext() =>
                this.MockUrlHelperFactory
                    .Setup(x => x.GetUrlHelper(this.ActionContext))
                    .Returns(this.MockUrlHelper.Object);

            public Mock<IUrlHelper> MockUrlHelper { get; } = new Mock<IUrlHelper>();

            public ActionContext ActionContext { get; } = new ActionContext();

            public string Email { get; } = "sample@example.com";

            public User User { get; private set; }

            public string Token { get; private set; }

            public string Link { get; private set; }

            public void SetupSuccess()
            {
                this.User = new User { Id = 31, Email = "sample-x@example.com" };

                this.SetupFindUserByEmail(this.Email, this.User);

                this.MockRandomTokenGenerator
                    .Setup(x => x.Generate())
                    .Returns(this.Token = "sample-token");

                this.MockUrlHelper
                    .Setup(x => x.Link(
                        "ResetPassword",
                        It.Is<object>(
                            o => this.Token.Equals(new RouteValueDictionary(o)["token"]))))
                    .Returns(this.Link = "https://example.com/reset-password/sample-token");
            }

            public void SetupUnrecognizedEmail() =>
                this.SetupFindUserByEmail(this.Email, this.User = null);

            public Task SendPasswordResetLink() =>
                this.AuthenticationManager.SendPasswordResetLink(this.ActionContext, this.Email);
        }
    }
}
