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
            var fixture = AuthenticateFixture.ForSuccess();

            await fixture.Authenticate();

            fixture.VerifyEventLogged(
                "authentication_success", fixture.User.Id, fixture.Email);
        }

        [Fact]
        public async Task AuthenticateReturnsUserOnSuccess()
        {
            var fixture = AuthenticateFixture.ForSuccess();

            var actual = await fixture.Authenticate();

            Assert.Equal(fixture.User, actual);
        }

        [Fact]
        public async Task AuthenticateLogsEventIfEmailIsUnrecognized()
        {
            var fixture = AuthenticateFixture.ForEmailNotFound();

            await fixture.Authenticate();

            fixture.VerifyEventLogged(
                "authentication_failure:unrecognized_email", null, fixture.Email);
        }

        [Fact]
        public async Task AuthenticateReturnsNullIfEmailIsUnrecognized()
        {
            var fixture = AuthenticateFixture.ForEmailNotFound();

            Assert.Null(await fixture.Authenticate());
        }

        [Fact]
        public async Task AuthenticateLogsEventIfUserHasNoPassword()
        {
            var fixture = AuthenticateFixture.ForUserHasNoPassword();

            await fixture.Authenticate();

            fixture.VerifyEventLogged(
                "authentication_failure:no_password_set", fixture.User.Id, fixture.Email);
        }

        [Fact]
        public async Task AuthenticateReturnsNullIfUserHasNoPassword()
        {
            var fixture = AuthenticateFixture.ForUserHasNoPassword();

            Assert.Null(await fixture.Authenticate());
        }

        [Fact]
        public async Task AuthenticateLogsEventIfPasswordIsIncorrect()
        {
            var fixture = AuthenticateFixture.ForPasswordIncorrect();

            await fixture.Authenticate();

            fixture.VerifyEventLogged(
                "authentication_failure:incorrect_password", fixture.User.Id, fixture.Email);
        }

        [Fact]
        public async Task AuthenticateReturnsNullIfPasswordIsIncorrect()
        {
            var fixture = AuthenticateFixture.ForPasswordIncorrect();

            Assert.Null(await fixture.Authenticate());
        }

        private class AuthenticateFixture : AuthenticationManagerFixture
        {
            private const long UserId = 29;
            private const string Password = "user-password";
            private const string HashedPassword = "hashed-password";

            private AuthenticateFixture(User user)
            {
                this.User = user;

                this.MockUserDataProvider
                    .Setup(x => x.FindUserByEmail(this.DbConnection, this.Email))
                    .ReturnsAsync(user);
            }

            public string Email { get; } = "user@example.com";

            public User User { get; }

            public static AuthenticateFixture ForSuccess() =>
                ForPasswordVerificationResult(PasswordVerificationResult.Success);

            public static AuthenticateFixture ForEmailNotFound() => new(null);

            public static AuthenticateFixture ForUserHasNoPassword() =>
                new(new User { Id = UserId, HashedPassword = null });

            public static AuthenticateFixture ForPasswordIncorrect() =>
                ForPasswordVerificationResult(PasswordVerificationResult.Failed);

            public Task<User> Authenticate() =>
                this.AuthenticationManager.Authenticate(this.Email, Password);

            private static AuthenticateFixture ForPasswordVerificationResult(
                PasswordVerificationResult result)
            {
                var user = new User { Id = UserId, HashedPassword = HashedPassword };

                var fixture = new AuthenticateFixture(user);

                fixture.MockPasswordHasher
                    .Setup(x => x.VerifyHashedPassword(user, HashedPassword, Password))
                    .Returns(result);

                return fixture;
            }
        }

        #endregion

        #region ChangePassword

        [Fact]
        public async Task ChangePasswordLogsEventIfUserHasNoPassword()
        {
            var fixture = new ChangePasswordFixture();

            fixture.User.HashedPassword = null;

            try
            {
                await fixture.ChangePassword();
            }
            catch (InvalidOperationException)
            {
            }

            fixture.VerifyEventLogged("password_change_failure:no_password_set", fixture.UserId);
        }

        [Fact]
        public async Task ChangePasswordThrowsIfUserHasNoPassword()
        {
            var fixture = new ChangePasswordFixture();

            fixture.User.HashedPassword = null;

            await Assert.ThrowsAsync<InvalidOperationException>(fixture.ChangePassword);
        }

        [Fact]
        public async Task ChangePasswordLogsEventIfCurrentPasswordDoesNotMatch()
        {
            var fixture = new ChangePasswordFixture();

            fixture.SetupVerifyHashedPassword(PasswordVerificationResult.Failed);

            await fixture.ChangePassword();

            fixture.VerifyEventLogged("password_change_failure:incorrect_password", fixture.UserId);
        }

        [Fact]
        public async Task ChangePasswordReturnsFalseIfCurrentPasswordDoesNotMatch()
        {
            var fixture = new ChangePasswordFixture();

            fixture.SetupVerifyHashedPassword(PasswordVerificationResult.Failed);

            Assert.False(await fixture.ChangePassword());
        }

        [Fact]
        public async Task ChangePasswordDoesNotChangePasswordIfCurrentPasswordDoesNotMatch()
        {
            var fixture = new ChangePasswordFixture();

            fixture.SetupVerifyHashedPassword(PasswordVerificationResult.Failed);

            await fixture.ChangePassword();

            fixture.MockPasswordHasher.Verify(
                x => x.HashPassword(null, fixture.NewPassword), Times.Never);
        }

        [Fact]
        public async Task ChangePasswordUpdatesUser()
        {
            var fixture = new ChangePasswordFixture();

            fixture.SetupVerifyHashedPassword(PasswordVerificationResult.Success);

            fixture.MockPasswordHasher
                .Setup(x => x.HashPassword(null, fixture.NewPassword))
                .Returns("sample-hashed-password");

            await fixture.ChangePassword();

            fixture.MockUserDataProvider.Verify(x => x.UpdatePassword(
                fixture.DbConnection,
                fixture.UserId,
                "sample-hashed-password",
                "sample-security-stamp",
                fixture.UtcNow));
        }

        [Fact]
        public async Task ChangePasswordDeletesPasswordResetTokens()
        {
            var fixture = new ChangePasswordFixture();

            fixture.SetupVerifyHashedPassword(PasswordVerificationResult.Success);

            await fixture.ChangePassword();

            fixture.MockPasswordResetTokenDataProvider.Verify(
                x => x.DeleteTokensForUser(fixture.DbConnection, fixture.UserId));
        }

        [Fact]
        public async Task ChangePasswordSendsPasswordChangeNotification()
        {
            var fixture = new ChangePasswordFixture();

            fixture.User.Email = "user@example.com";

            fixture.SetupVerifyHashedPassword(PasswordVerificationResult.Success);

            await fixture.ChangePassword();

            fixture.MockAuthenticationMailer.Verify(
                x => x.SendPasswordChangeNotification("user@example.com"));
        }

        [Fact]
        public async Task ChangePasswordLogsEvent()
        {
            var fixture = new ChangePasswordFixture();

            fixture.SetupVerifyHashedPassword(PasswordVerificationResult.Success);

            await fixture.ChangePassword();

            fixture.VerifyEventLogged("password_change_success", fixture.UserId);
        }

        [Fact]
        public async Task ChangePasswordSignsInUpdatedPrincipal()
        {
            var fixture = new ChangePasswordFixture();

            fixture.SetupVerifyHashedPassword(PasswordVerificationResult.Success);

            fixture.MockAuthenticationService
                .Setup(x => x.SignInAsync(
                    fixture.HttpContext,
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    It.IsAny<ClaimsPrincipal>(),
                    null))
                .Callback<HttpContext, string, ClaimsPrincipal, AuthenticationProperties>(
                    (contextArg, scheme, principal, properties) =>
                    {
                        Assert.Equal(
                            fixture.UserId.ToString(CultureInfo.InvariantCulture),
                            principal.FindFirstValue(ClaimTypes.NameIdentifier));
                        Assert.Equal(
                            fixture.Email,
                            principal.FindFirstValue(ClaimTypes.Email));
                        Assert.Equal(
                            fixture.SecurityStamp,
                            principal.FindFirstValue(CustomClaimTypes.SecurityStamp));

                        Assert.Equal(fixture.Email, principal.Identity.Name);
                    })
                .Returns(Task.CompletedTask)
                .Verifiable();

            await fixture.ChangePassword();

            fixture.MockAuthenticationService.Verify();
        }

        [Fact]
        public async Task ChangePasswordReturnsTrue()
        {
            var fixture = new ChangePasswordFixture();

            fixture.SetupVerifyHashedPassword(PasswordVerificationResult.Success);

            Assert.True(await fixture.ChangePassword());
        }

        private class ChangePasswordFixture : AuthenticationManagerFixture
        {
            public ChangePasswordFixture()
            {
                this.User = new()
                {
                    Id = this.UserId,
                    Email = this.Email,
                    HashedPassword = this.HashedCurrentPassword,
                };

                this.HttpContext.SetCurrentUser(this.User);

                this.MockRandomTokenGenerator
                    .Setup(x => x.Generate(2))
                    .Returns(this.SecurityStamp);
            }

            public DefaultHttpContext HttpContext { get; } = new();

            public User User { get; }

            public long UserId { get; } = 43;

            public string Email { get; } = "sample@example.com";

            public string CurrentPassword { get; } = "current-password";

            public string HashedCurrentPassword { get; } = "hashed-current-password";

            public string NewPassword { get; } = "new-password";

            public string SecurityStamp { get; } = "sample-security-stamp";

            public void SetupVerifyHashedPassword(PasswordVerificationResult result) =>
                this.MockPasswordHasher
                    .Setup(x => x.VerifyHashedPassword(
                        this.User, this.HashedCurrentPassword, this.CurrentPassword))
                    .Returns(result);

            public Task<bool> ChangePassword() => this.AuthenticationManager.ChangePassword(
                this.HttpContext, this.CurrentPassword, this.NewPassword);
        }

        #endregion

        #region PasswordResetTokenIsValid

        [Fact]
        public async Task PasswordResetTokenIsValidDeletesExpiredTokens()
        {
            var fixture = PasswordResetTokenIsValidFixture.ForValidToken();

            await fixture.PasswordResetTokenIsValid();

            fixture.MockPasswordResetTokenDataProvider.Verify(
                x => x.DeleteExpiredTokens(fixture.DbConnection, fixture.UtcNow.AddDays(-1)));
        }

        [Fact]
        public async Task PasswordResetTokenIsValidDoesNotLogEventIfValid()
        {
            var fixture = PasswordResetTokenIsValidFixture.ForValidToken();

            await fixture.PasswordResetTokenIsValid();

            fixture.MockAuthenticationEventDataProvider.Verify(
                x => x.LogEvent(
                    fixture.DbConnection,
                    fixture.UtcNow,
                    "password_reset_failure:invalid_token",
                    null,
                    null),
                Times.Never);
        }

        [Fact]
        public async Task PasswordResetTokenIsValidReturnsTrueIfValid()
        {
            var fixture = PasswordResetTokenIsValidFixture.ForValidToken();

            Assert.True(await fixture.PasswordResetTokenIsValid());
        }

        [Fact]
        public async Task PasswordResetTokenIsValidLogsEventIfInvalid()
        {
            var fixture = PasswordResetTokenIsValidFixture.ForInvalidToken();

            await fixture.PasswordResetTokenIsValid();

            fixture.VerifyEventLogged("password_reset_failure:invalid_token");
        }

        [Fact]
        public async Task PasswordResetTokenIsValidReturnsFalseIfInvalid()
        {
            var fixture = PasswordResetTokenIsValidFixture.ForInvalidToken();

            await fixture.PasswordResetTokenIsValid();

            Assert.False(await fixture.PasswordResetTokenIsValid());
        }

        private class PasswordResetTokenIsValidFixture : AuthenticationManagerFixture
        {
            private const string Token = "password-reset-token";

            private PasswordResetTokenIsValidFixture(long? userId) =>
                this.SetupGetUserIdForToken(Token, userId);

            public static PasswordResetTokenIsValidFixture ForValidToken() => new(43);

            public static PasswordResetTokenIsValidFixture ForInvalidToken() => new(null);

            public Task<bool> PasswordResetTokenIsValid() =>
                this.AuthenticationManager.PasswordResetTokenIsValid(Token);
        }

        #endregion

        #region ResetPassword

        [Fact]
        public async Task ResetPasswordDeletesExpiredPasswordResetTokens()
        {
            var fixture = ResetPasswordFixture.ForSuccess();

            await fixture.ResetPassword();

            fixture.MockPasswordResetTokenDataProvider.Verify(
                x => x.DeleteExpiredTokens(fixture.DbConnection, fixture.UtcNow.AddDays(-1)));
        }

        [Fact]
        public async Task ResetPasswordLogsEventIfTokenIsInvalid()
        {
            var fixture = ResetPasswordFixture.ForInvalidToken();

            try
            {
                await fixture.ResetPassword();
            }
            catch (InvalidTokenException)
            {
            }

            fixture.VerifyEventLogged("password_reset_failure:invalid_token");
        }

        [Fact]
        public async Task ResetPasswordThrowsIfTokenIsInvalid()
        {
            var fixture = ResetPasswordFixture.ForInvalidToken();

            await Assert.ThrowsAsync<InvalidTokenException>(fixture.ResetPassword);
        }

        [Fact]
        public async Task ResetPasswordUpdatesUserOnSuccess()
        {
            var fixture = ResetPasswordFixture.ForSuccess();

            await fixture.ResetPassword();

            fixture.MockUserDataProvider.Verify(x => x.UpdatePassword(
                fixture.DbConnection,
                fixture.UserId.Value,
                fixture.NewHashedPassword,
                fixture.NewSecurityStamp,
                fixture.UtcNow));
        }

        [Fact]
        public async Task ResetPasswordDeletesPasswordResetTokensOnSuccess()
        {
            var fixture = ResetPasswordFixture.ForSuccess();

            await fixture.ResetPassword();

            fixture.MockPasswordResetTokenDataProvider.Verify(
                x => x.DeleteTokensForUser(fixture.DbConnection, fixture.UserId.Value));
        }

        [Fact]
        public async Task ResetPasswordSendsPasswordChangeNotificationOnSuccess()
        {
            var fixture = ResetPasswordFixture.ForSuccess();

            await fixture.ResetPassword();

            fixture.MockAuthenticationMailer.Verify(
                x => x.SendPasswordChangeNotification(fixture.User.Email));
        }

        [Fact]
        public async Task ResetPasswordLogsEventOnSuccess()
        {
            var fixture = ResetPasswordFixture.ForSuccess();

            await fixture.ResetPassword();

            fixture.VerifyEventLogged("password_reset_success", fixture.UserId);
        }

        [Fact]
        public async Task ResetPasswordReturnsUserOnSuccess()
        {
            var fixture = ResetPasswordFixture.ForSuccess();

            var actual = await fixture.ResetPassword();

            Assert.Equal(fixture.User, actual);
        }

        private class ResetPasswordFixture : AuthenticationManagerFixture
        {
            private const string NewPassword = "new-password";

            private ResetPasswordFixture(long? userId)
            {
                this.UserId = userId;
                this.SetupGetUserIdForToken(this.Token, userId);
            }

            private ResetPasswordFixture(long userId, User user)
                : this(userId)
            {
                this.User = user;

                this.SetupGetUser(userId, user);

                this.MockPasswordHasher
                    .Setup(x => x.HashPassword(null, NewPassword))
                    .Returns(this.NewHashedPassword);

                this.MockRandomTokenGenerator
                    .Setup(x => x.Generate(2))
                    .Returns(this.NewSecurityStamp);
            }

            public string Token { get; } = "password-reset-token";

            public long? UserId { get; }

            public User User { get; }

            public string NewHashedPassword { get; } = "new-hashed-password";

            public string NewSecurityStamp { get; } = "new-security-stamp";

            public static ResetPasswordFixture ForInvalidToken() => new(null);

            public static ResetPasswordFixture ForSuccess() =>
                new(23, new() { Email = "user@example.com" });

            public Task<User> ResetPassword() =>
                this.AuthenticationManager.ResetPassword(this.Token, NewPassword);
        }

        #endregion

        #region SendPasswordResetLink

        [Fact]
        public async Task SendPasswordResetLinkInsertsPasswordResetTokenOnSuccess()
        {
            var fixture = SendPasswordResetLinkFixture.ForSuccess();

            await fixture.SendPasswordResetLink();

            fixture.MockPasswordResetTokenDataProvider.Verify(x => x.InsertToken(
                fixture.DbConnection, fixture.User.Id, fixture.Token, fixture.UtcNow));
        }

        [Fact]
        public async Task SendPasswordResetLinkSendsLinkToUserOnSuccess()
        {
            var fixture = SendPasswordResetLinkFixture.ForSuccess();

            await fixture.SendPasswordResetLink();

            fixture.MockAuthenticationMailer.Verify(
                x => x.SendPasswordResetLink(fixture.User.Email, fixture.Link));
        }

        [Fact]
        public async Task SendPasswordResetLinkLogsEventOnSuccess()
        {
            var fixture = SendPasswordResetLinkFixture.ForSuccess();

            await fixture.SendPasswordResetLink();

            fixture.VerifyEventLogged(
                "password_reset_link_sent", fixture.User.Id, fixture.User.Email);
        }

        [Fact]
        public async Task SendPasswordResetLinkDoesNotSendLinkIfEmailIsUnrecognized()
        {
            var fixture = SendPasswordResetLinkFixture.ForUnrecognizedEmail();

            await fixture.SendPasswordResetLink();

            fixture.MockAuthenticationMailer.Verify(
                x => x.SendPasswordResetLink(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task SendPasswordResetLinkLogsEventIfEmailIsUnrecognized()
        {
            var fixture = SendPasswordResetLinkFixture.ForUnrecognizedEmail();

            await fixture.SendPasswordResetLink();

            fixture.VerifyEventLogged(
                "password_reset_failure:unrecognized_email", null, fixture.SuppliedEmail);
        }

        private class SendPasswordResetLinkFixture : AuthenticationManagerFixture
        {
            private SendPasswordResetLinkFixture(User user)
            {
                this.User = user;

                this.MockUrlHelperFactory
                    .Setup(x => x.GetUrlHelper(this.ActionContext))
                    .Returns(this.MockUrlHelper.Object);

                this.MockUserDataProvider
                    .Setup(x => x.FindUserByEmail(this.DbConnection, this.SuppliedEmail))
                    .ReturnsAsync(user);
            }

            public Mock<IUrlHelper> MockUrlHelper { get; } = new();

            public ActionContext ActionContext { get; } = new();

            public string SuppliedEmail { get; } = "supplied-email@example.com";

            public User User { get; }

            public string Token { get; } = "password-reset-token";

            public string Link { get; } = "https://example.com/reset-password/token";

            public static SendPasswordResetLinkFixture ForSuccess()
            {
                var fixture = new SendPasswordResetLinkFixture(
                    new() { Id = 31, Email = "user-email@example.com" });

                fixture.MockRandomTokenGenerator
                    .Setup(x => x.Generate(12))
                    .Returns(fixture.Token);

                fixture.MockUrlHelper
                    .Setup(x => x.Link(
                        "ResetPassword",
                        It.Is<object>(
                            o => fixture.Token.Equals(new RouteValueDictionary(o)["token"]))))
                    .Returns(fixture.Link);

                return fixture;
            }

            public static SendPasswordResetLinkFixture ForUnrecognizedEmail() => new(null);

            public Task SendPasswordResetLink() =>
                this.AuthenticationManager.SendPasswordResetLink(
                    this.ActionContext, this.SuppliedEmail);
        }

        #endregion

        #region SignIn

        [Fact]
        public async Task SignInSignsInPrincipal()
        {
            var fixture = new SignInFixture();

            fixture.MockAuthenticationService
                .Setup(x => x.SignInAsync(
                    fixture.HttpContext,
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    It.IsAny<ClaimsPrincipal>(),
                    null))
                .Callback<HttpContext, string, ClaimsPrincipal, AuthenticationProperties>(
                    (contextArg, scheme, principal, properties) =>
                    {
                        Assert.Equal(
                            fixture.UserId.ToString(CultureInfo.InvariantCulture),
                            principal.FindFirstValue(ClaimTypes.NameIdentifier));
                        Assert.Equal(
                            fixture.Email,
                            principal.FindFirstValue(ClaimTypes.Email));
                        Assert.Equal(
                            fixture.SecurityStamp,
                            principal.FindFirstValue(CustomClaimTypes.SecurityStamp));

                        Assert.Equal(fixture.Email, principal.Identity.Name);
                    })
                .Returns(Task.CompletedTask)
                .Verifiable();

            await fixture.SignIn();

            fixture.MockAuthenticationService.Verify();
        }

        [Fact]
        public async Task SignInSetsCurrentUser()
        {
            var fixture = new SignInFixture();

            await fixture.SignIn();

            Assert.Equal(fixture.User, fixture.HttpContext.GetCurrentUser());
        }

        [Fact]
        public async Task SignInLogsEvent()
        {
            var fixture = new SignInFixture();

            await fixture.SignIn();

            fixture.VerifyEventLogged("sign_in", fixture.UserId);
        }

        private class SignInFixture : AuthenticationManagerFixture
        {
            public SignInFixture() => this.User = new()
            {
                Id = this.UserId,
                Email = this.Email,
                SecurityStamp = this.SecurityStamp,
            };

            public DefaultHttpContext HttpContext { get; } = new();

            public User User { get; }

            public long UserId { get; } = 6;

            public string Email { get; } = "sample@example.com";

            public string SecurityStamp { get; } = "sample-security-stamp";

            public Task SignIn() => this.AuthenticationManager.SignIn(this.HttpContext, this.User);
        }

        #endregion

        #region SignOut

        [Fact]
        public async Task SignOutSignsOutUser()
        {
            var fixture = SignOutFixture.ForUserSignedIn();

            await fixture.SignOut();

            fixture.MockAuthenticationService.Verify(x => x.SignOutAsync(
                fixture.HttpContext, CookieAuthenticationDefaults.AuthenticationScheme, null));
        }

        [Fact]
        public async Task SignOutLogsEventIfUserPreviouslySignedIn()
        {
            var fixture = SignOutFixture.ForUserSignedIn();

            await fixture.SignOut();

            fixture.VerifyEventLogged("sign_out", fixture.UserId);
        }

        [Fact]
        public async Task SignOutDoesNotLogsEventIfUserPreviouslySignedOut()
        {
            var fixture = SignOutFixture.ForNoUserSignedIn();

            await fixture.SignOut();

            fixture.MockAuthenticationEventDataProvider.Verify(
                x => x.LogEvent(fixture.DbConnection, fixture.UtcNow, "sign_out", null, null),
                Times.Never);
        }

        private class SignOutFixture : AuthenticationManagerFixture
        {
            private SignOutFixture(long? userId) => this.UserId = userId;

            public DefaultHttpContext HttpContext { get; } = new();

            public long? UserId { get; }

            public static SignOutFixture ForNoUserSignedIn() => new(null);

            public static SignOutFixture ForUserSignedIn()
            {
                var fixture = new SignOutFixture(76);

                fixture.HttpContext.User = new ClaimsPrincipal(
                    new ClaimsIdentity(
                        new[] { new Claim(ClaimTypes.NameIdentifier, "76") },
                        CookieAuthenticationDefaults.AuthenticationScheme));

                return fixture;
            }

            public Task SignOut() => this.AuthenticationManager.SignOut(this.HttpContext);
        }

        #endregion

        #region ValidatePrincipal

        [Fact]
        public async Task ValidatePrincipalDoesNotRejectPrincipalWhenUnauthenticated()
        {
            var fixture = new ValidatePrincipalFixture();

            fixture.SetupUnauthenticated();

            await fixture.ValidatePrincipal();

            Assert.Same(fixture.Principal, fixture.CookieContext.Principal);
        }

        [Fact]
        public async Task ValidatePrincipalSetsCurrentUserWhenStampIsCorrect()
        {
            var fixture = new ValidatePrincipalFixture();

            fixture.SetupCorrectStamp();

            await fixture.ValidatePrincipal();

            Assert.Equal(fixture.User, fixture.HttpContext.GetCurrentUser());
        }

        [Fact]
        public async Task ValidatePrincipalDoesNotRejectPrincipalWhenStampIsCorrect()
        {
            var fixture = new ValidatePrincipalFixture();

            fixture.SetupCorrectStamp();

            await fixture.ValidatePrincipal();

            Assert.Same(fixture.Principal, fixture.CookieContext.Principal);
        }

        [Fact]
        public async Task ValidatePrincipalRejectsPrincipalWhenStampIsIncorrect()
        {
            var fixture = new ValidatePrincipalFixture();

            fixture.SetupIncorrectStamp();

            await fixture.ValidatePrincipal();

            Assert.Null(fixture.CookieContext.Principal);
        }

        [Fact]
        public async Task ValidatePrincipalSignsUserOutWhenStampIsIncorrect()
        {
            var fixture = new ValidatePrincipalFixture();

            fixture.SetupIncorrectStamp();

            await fixture.ValidatePrincipal();

            fixture.MockAuthenticationService.Verify(
                x => x.SignOutAsync(
                    fixture.HttpContext, CookieAuthenticationDefaults.AuthenticationScheme, null));
        }

        private class ValidatePrincipalFixture : AuthenticationManagerFixture
        {
            public CookieValidatePrincipalContext CookieContext { get; private set; }

            public DefaultHttpContext HttpContext { get; } = new();

            public ClaimsPrincipal Principal { get; private set; }

            public User User { get; private set; }

            public void SetupUnauthenticated() => this.SetupCookieContext();

            public void SetupCorrectStamp() =>
                this.SetupAuthenticated("sample-security-stamp", "sample-security-stamp");

            public void SetupIncorrectStamp() =>
                this.SetupAuthenticated("principal-security-stamp", "user-security-stamp");

            public Task ValidatePrincipal() =>
                this.AuthenticationManager.ValidatePrincipal(this.CookieContext);

            private void SetupCookieContext(params Claim[] claims)
            {
                this.Principal = new(new ClaimsIdentity(claims));

                var scheme = new AuthenticationScheme(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    null,
                    typeof(CookieAuthenticationHandler));
                var ticket = new AuthenticationTicket(this.Principal, null);

                this.CookieContext = new(this.HttpContext, scheme, new(), ticket);
            }

            private void SetupAuthenticated(string principalSecurityStamp, string userSecurityStamp)
            {
                this.SetupCookieContext(
                    new(ClaimTypes.NameIdentifier, "34"),
                    new(CustomClaimTypes.SecurityStamp, principalSecurityStamp));

                this.User = new() { SecurityStamp = userSecurityStamp };

                this.SetupGetUser(34, this.User);
            }
        }

        #endregion

        private class AuthenticationManagerFixture
        {
            public AuthenticationManagerFixture()
            {
                var clock = Mock.Of<IClock>(x => x.UtcNow == this.UtcNow);
                var dbConnectionSource = Mock.Of<IDbConnectionSource>(
                    x => x.OpenConnection() == Task.FromResult(this.DbConnection));

                this.AuthenticationManager = new(
                    this.MockAuthenticationEventDataProvider.Object,
                    this.MockAuthenticationMailer.Object,
                    this.MockAuthenticationService.Object,
                    clock,
                    dbConnectionSource,
                    Mock.Of<ILogger<AuthenticationManager>>(),
                    this.MockPasswordHasher.Object,
                    this.MockPasswordResetTokenDataProvider.Object,
                    this.MockRandomTokenGenerator.Object,
                    this.MockUrlHelperFactory.Object,
                    this.MockUserDataProvider.Object);
            }

            public Mock<IAuthenticationEventDataProvider> MockAuthenticationEventDataProvider { get; } = new();

            public AuthenticationManager AuthenticationManager { get; }

            public DbConnection DbConnection { get; } = Mock.Of<DbConnection>();

            public Mock<IAuthenticationMailer> MockAuthenticationMailer { get; } = new();

            public Mock<IAuthenticationService> MockAuthenticationService { get; } = new();

            public Mock<IPasswordHasher<User>> MockPasswordHasher { get; } = new();

            public Mock<IPasswordResetTokenDataProvider> MockPasswordResetTokenDataProvider { get; } = new();

            public Mock<IRandomTokenGenerator> MockRandomTokenGenerator { get; } = new();

            public Mock<IUrlHelperFactory> MockUrlHelperFactory { get; } = new();

            public Mock<IUserDataProvider> MockUserDataProvider { get; } = new();

            public DateTime UtcNow { get; } = new(2000, 1, 2, 3, 4, 5, DateTimeKind.Utc);

            public void SetupGetUser(long id, User user) =>
                this.MockUserDataProvider
                    .Setup(x => x.GetUser(this.DbConnection, id))
                    .ReturnsAsync(user);

            public void SetupGetUserIdForToken(string token, long? userId) =>
                this.MockPasswordResetTokenDataProvider
                    .Setup(x => x.GetUserIdForToken(this.DbConnection, token))
                    .ReturnsAsync(userId);

            public void VerifyEventLogged(
                string eventName, long? userId = null, string email = null) =>
                this.MockAuthenticationEventDataProvider.Verify(x => x.LogEvent(
                    this.DbConnection, this.UtcNow, eventName, userId, email));
        }
    }
}
