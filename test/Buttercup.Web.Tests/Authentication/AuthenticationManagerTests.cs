using System;
using System.Data.Common;
using System.Globalization;
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
        public async Task AuthenticateLogsEventOnSuccess()
        {
            var context = new AuthenticateContext();

            context.SetupSuccess();

            await context.Authenticate();

            context.MockAuthenticationEventDataProvider.Verify(x => x.LogEvent(
                context.MockConnection.Object,
                "authentication_success",
                context.UserId,
                "sample@example.com"));
        }

        [Fact]
        public async Task AuthenticateReturnsUserOnSuccess()
        {
            var context = new AuthenticateContext();

            context.SetupSuccess();

            var actual = await context.Authenticate();

            Assert.Equal(context.User, actual);
        }

        [Fact]
        public async Task AuthenticateLogsEventIfEmailIsUnrecognized()
        {
            var context = new AuthenticateContext();

            context.SetupEmailNotFound();

            await context.Authenticate();

            context.MockAuthenticationEventDataProvider.Verify(x => x.LogEvent(
                context.MockConnection.Object,
                "authentication_failure:unrecognized_email",
                null,
                "sample@example.com"));
        }

        [Fact]
        public async Task AuthenticateReturnsNullIfEmailIsUnrecognized()
        {
            var context = new AuthenticateContext();

            context.SetupEmailNotFound();

            Assert.Null(await context.Authenticate());
        }

        [Fact]
        public async Task AuthenticateLogsEventIfUserHasNoPassword()
        {
            var context = new AuthenticateContext();

            context.SetupUserHasNoPassword();

            await context.Authenticate();

            context.MockAuthenticationEventDataProvider.Verify(x => x.LogEvent(
                context.MockConnection.Object,
                "authentication_failure:no_password_set",
                context.UserId,
                "sample@example.com"));
        }

        [Fact]
        public async Task AuthenticateReturnsNullIfUserHasNoPassword()
        {
            var context = new AuthenticateContext();

            context.SetupUserHasNoPassword();

            Assert.Null(await context.Authenticate());
        }

        [Fact]
        public async Task AuthenticateLogsEventIfPasswordIsIncorrect()
        {
            var context = new AuthenticateContext();

            context.SetupPasswordIncorrect();

            await context.Authenticate();

            context.MockAuthenticationEventDataProvider.Verify(x => x.LogEvent(
                context.MockConnection.Object,
                "authentication_failure:incorrect_password",
                context.UserId,
                "sample@example.com"));
        }

        [Fact]
        public async Task AuthenticateReturnsNullIfPasswordIsIncorrect()
        {
            var context = new AuthenticateContext();

            context.SetupPasswordIncorrect();

            Assert.Null(await context.Authenticate());
        }

        #endregion

        #region GetCurrentUser

        [Fact]
        public async Task GetCurrentUserReturnsAndCachesUserIfAuthenticated()
        {
            var context = new Context();

            var expected = new User();

            context.SetupGetUser(12, expected);

            var principal = new ClaimsPrincipal(new ClaimsIdentity(
                new Claim[] { new Claim(ClaimTypes.NameIdentifier, "12") },
                CookieAuthenticationDefaults.AuthenticationScheme));

            var httpContext = new DefaultHttpContext { User = principal };

            var actual = await context.AuthenticationManager.GetCurrentUser(httpContext);

            Assert.Equal(expected, actual);
            Assert.Equal(expected, httpContext.Items[typeof(User)]);
        }

        [Fact]
        public async Task GetCurrentUserReturnsAndCachesNullIfUnauthenticated()
        {
            var httpContext = new DefaultHttpContext();

            Assert.Null(await new Context().AuthenticationManager.GetCurrentUser(httpContext));
            Assert.Null(httpContext.Items[typeof(User)]);
        }

        [Fact]
        public async Task GetCurrentUserReturnsUserFromRequestItemsOnceCached()
        {
            var context = new Context();

            var expected = new User();

            var httpContext = new DefaultHttpContext();
            httpContext.Items[typeof(User)] = expected;

            var actual = await context.AuthenticationManager.GetCurrentUser(httpContext);

            Assert.Equal(expected, actual);
        }

        #endregion

        #region ChangePassword

        [Fact]
        public async Task ChangePasswordThrowsIfUserHasNoPassword()
        {
            var context = new ChangePasswordContext();

            context.User.HashedPassword = null;

            await Assert.ThrowsAsync<InvalidOperationException>(context.ChangePassword);
        }

        [Fact]
        public async Task ChangePasswordReturnsFalseIfCurrentPasswordDoesNotMatch()
        {
            var context = new ChangePasswordContext();

            context.SetupVerifyHashedPassword(PasswordVerificationResult.Failed);

            Assert.False(await context.ChangePassword());
        }

        [Fact]
        public async Task ChangePasswordDoesNotChangePasswordIfCurrentPasswordDoesNotMatch()
        {
            var context = new ChangePasswordContext();

            context.SetupVerifyHashedPassword(PasswordVerificationResult.Failed);

            await context.ChangePassword();

            context.MockPasswordHasher.Verify(
                x => x.HashPassword(null, context.NewPassword), Times.Never);
        }

        [Fact]
        public async Task ChangePasswordUpdatesPasswordIfCurrentPasswordMatches()
        {
            var context = new ChangePasswordContext();

            context.SetupVerifyHashedPassword(PasswordVerificationResult.Success);

            context.MockPasswordHasher
                .Setup(x => x.HashPassword(null, context.NewPassword))
                .Returns("sample-hashed-password");

            await context.ChangePassword();

            context.MockUserDataProvider.Verify(x => x.UpdatePassword(
                context.MockConnection.Object, context.UserId, "sample-hashed-password"));
        }

        [Fact]
        public async Task ChangePasswordDeletesPasswordResetTokens()
        {
            var context = new ChangePasswordContext();

            context.SetupVerifyHashedPassword(PasswordVerificationResult.Success);

            await context.ChangePassword();

            context.MockPasswordResetTokenDataProvider.Verify(
                x => x.DeleteTokensForUser(context.MockConnection.Object, context.UserId));
        }

        [Fact]
        public async Task ChangePasswordSendsPasswordChangeNotification()
        {
            var context = new ChangePasswordContext();

            context.User.Email = "user@example.com";

            context.SetupVerifyHashedPassword(PasswordVerificationResult.Success);

            await context.ChangePassword();

            context.MockAuthenticationMailer.Verify(
                x => x.SendPasswordChangeNotification("user@example.com"));
        }

        [Fact]
        public async Task ChangePasswordReturnsTrueIfCurrentPasswordMatches()
        {
            var context = new ChangePasswordContext();

            context.SetupVerifyHashedPassword(PasswordVerificationResult.Success);

            Assert.True(await context.ChangePassword());
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
        public async Task ResetPasswordSendsPasswordChangeNotification()
        {
            var context = new ResetPasswordContext();

            context.SetupSuccess();

            await context.ResetPassword();

            context.MockAuthenticationMailer.Verify(
                x => x.SendPasswordChangeNotification(context.Email));
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
            var context = new SignInContext();

            ClaimsPrincipal principal = null;

            context.MockAuthenticationService
                .Setup(x => x.SignInAsync(
                    context.HttpContext,
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    It.IsAny<ClaimsPrincipal>(),
                    null))
                .Callback<HttpContext, string, ClaimsPrincipal, AuthenticationProperties>(
                   (contextArg, scheme, principalArg, properties) => principal = principalArg)
                .Returns(Task.CompletedTask);

            await context.SignIn();

            Assert.Equal(
                context.UserId.ToString(CultureInfo.InvariantCulture),
                principal.FindFirstValue(ClaimTypes.NameIdentifier));

            Assert.Equal(context.Email, principal.Identity.Name);
        }

        [Fact]
        public async Task SignInLogsEvent()
        {
            var context = new SignInContext();

            await context.SignIn();

            context.MockAuthenticationEventDataProvider.Verify(x => x.LogEvent(
                context.MockConnection.Object, "sign_in", context.UserId, null));
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
                    this.MockAuthenticationEventDataProvider.Object,
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

            public Mock<IAuthenticationEventDataProvider> MockAuthenticationEventDataProvider
            { get; } = new Mock<IAuthenticationEventDataProvider>();

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

            public void SetupGetUser(long id, User user) =>
                this.MockUserDataProvider
                    .Setup(x => x.GetUser(this.MockConnection.Object, id))
                    .ReturnsAsync(user);

            public void SetupGetUserIdForToken(string token, long? userId) =>
                this.MockPasswordResetTokenDataProvider
                    .Setup(x => x.GetUserIdForToken(this.MockConnection.Object, token))
                    .ReturnsAsync(userId);
        }

        private class AuthenticateContext : Context
        {
            public AuthenticateContext() =>
                this.MockUserDataProvider
                    .Setup(x => x.FindUserByEmail(this.MockConnection.Object, this.Email))
                    .ReturnsAsync(() => this.User);

            public string Email { get; } = "sample@example.com";

            public User User { get; private set; }

            public long UserId { get; private set; }

            public void SetupEmailNotFound() => this.User = null;

            public void SetupUserHasNoPassword() => this.User = new User
            {
                Id = this.UserId = 29,
                HashedPassword = null
            };

            public void SetupPasswordIncorrect()
            {
                this.User = new User
                {
                    Id = this.UserId = 45,
                    HashedPassword = "sample-hashed-password",
                };

                this.MockPasswordHasher
                    .Setup(x => x.VerifyHashedPassword(
                        this.User, "sample-hashed-password", "sample-password"))
                    .Returns(PasswordVerificationResult.Failed);
            }

            public void SetupSuccess()
            {
                this.User = new User
                {
                    Id = this.UserId = 52,
                    HashedPassword = "sample-hashed-password",
                };

                this.MockPasswordHasher
                    .Setup(x => x.VerifyHashedPassword(
                        this.User, "sample-hashed-password", "sample-password"))
                    .Returns(PasswordVerificationResult.Success);
            }

            public Task<User> Authenticate() =>
                this.AuthenticationManager.Authenticate("sample@example.com", "sample-password");
        }

        private class ChangePasswordContext : Context
        {
            public ChangePasswordContext() => this.User = new User
            {
                Id = this.UserId,
                HashedPassword = this.HashedCurrentPassword,
            };

            public User User { get; }

            public long UserId { get; } = 43;

            public string CurrentPassword { get; } = "current-password";

            public string HashedCurrentPassword { get; } = "hashed-current-password";

            public string NewPassword { get; } = "new-password";

            public string HashedNewPassword { get; private set; }

            public void SetupVerifyHashedPassword(PasswordVerificationResult result) =>
                this.MockPasswordHasher
                    .Setup(x => x.VerifyHashedPassword(
                        this.User, this.HashedCurrentPassword, this.CurrentPassword))
                    .Returns(result);

            public Task<bool> ChangePassword() => this.AuthenticationManager.ChangePassword(
                this.User, this.CurrentPassword, this.NewPassword);
        }

        private class ResetPasswordContext : Context
        {
            public string Token { get; } = "sample-token";

            public string NewPassword { get; } = "sample-password";

            public long? UserId { get; private set; }

            public User User { get; private set; }

            public string Email { get; private set; }

            public void SetupInvalidToken() =>
                this.SetupGetUserIdForToken(this.Token, this.UserId = null);

            public void SetupSuccess()
            {
                this.UserId = 23;
                this.User = new User { Email = this.Email = "user@example.com" };

                this.SetupGetUserIdForToken(this.Token, this.UserId);
                this.SetupGetUser(this.UserId.Value, this.User);
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

            private void SetupFindUserByEmail(string email, User user) =>
                this.MockUserDataProvider
                    .Setup(x => x.FindUserByEmail(this.MockConnection.Object, email))
                    .ReturnsAsync(user);
        }

        private class SignInContext : Context
        {
            public SignInContext() => this.User = new User { Id = this.UserId, Email = this.Email };

            public HttpContext HttpContext { get; } = new DefaultHttpContext();

            public User User { get; }

            public long UserId { get; } = 6;

            public string Email { get; } = "sample@example.com";

            public Task SignIn() => this.AuthenticationManager.SignIn(this.HttpContext, this.User);
        }
    }
}
