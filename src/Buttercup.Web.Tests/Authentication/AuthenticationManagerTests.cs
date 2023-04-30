using System.Globalization;
using System.Security.Claims;
using Buttercup.DataAccess;
using Buttercup.EntityModel;
using Buttercup.TestUtils;
using Buttercup.Web.TestUtils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using MySqlConnector;
using Xunit;

namespace Buttercup.Web.Authentication;

public class AuthenticationManagerTests
{
    #region Authenticate

    [Fact]
    public async Task AuthenticateLogsOnSuccess()
    {
        var fixture = AuthenticateFixture.ForSuccess();

        await fixture.Authenticate();

        var user = fixture.User!;

        fixture.Logger.AssertSingleEntry(
            LogLevel.Information, $"User {user.Id} ({user.Email}) successfully authenticated");

        fixture.AssertAuthenticationEventLogged(
            "authentication_success", user.Id, fixture.SuppliedEmail);
    }

    [Fact]
    public async Task AuthenticateReturnsUserOnSuccess()
    {
        var fixture = AuthenticateFixture.ForSuccess();

        var actual = await fixture.Authenticate();

        Assert.Equal(fixture.User, actual);
    }

    [Fact]
    public async Task AuthenticateLogsIfEmailIsUnrecognized()
    {
        var fixture = AuthenticateFixture.ForEmailNotFound();

        await fixture.Authenticate();

        fixture.Logger.AssertSingleEntry(
            LogLevel.Information,
            $"Authentication failed; no user with email {fixture.SuppliedEmail}");

        fixture.AssertAuthenticationEventLogged(
            "authentication_failure:unrecognized_email", null, fixture.SuppliedEmail);
    }

    [Fact]
    public async Task AuthenticateReturnsNullIfEmailIsUnrecognized()
    {
        var fixture = AuthenticateFixture.ForEmailNotFound();

        Assert.Null(await fixture.Authenticate());
    }

    [Fact]
    public async Task AuthenticateLogsIfUserHasNoPassword()
    {
        var fixture = AuthenticateFixture.ForUserHasNoPassword();

        await fixture.Authenticate();

        var user = fixture.User!;

        fixture.Logger.AssertSingleEntry(
            LogLevel.Information,
            $"Authentication failed; no password set for user {user.Id} ({user.Email})");

        fixture.AssertAuthenticationEventLogged(
            "authentication_failure:no_password_set", user.Id, fixture.SuppliedEmail);
    }

    [Fact]
    public async Task AuthenticateReturnsNullIfUserHasNoPassword()
    {
        var fixture = AuthenticateFixture.ForUserHasNoPassword();

        Assert.Null(await fixture.Authenticate());
    }

    [Fact]
    public async Task AuthenticateLogsIfPasswordIsIncorrect()
    {
        var fixture = AuthenticateFixture.ForPasswordIncorrect();

        await fixture.Authenticate();

        var user = fixture.User!;

        fixture.Logger.AssertSingleEntry(
            LogLevel.Information,
            $"Authentication failed; incorrect password for user {user.Id} ({user.Email})");

        fixture.AssertAuthenticationEventLogged(
            "authentication_failure:incorrect_password", user.Id, fixture.SuppliedEmail);
    }

    [Fact]
    public async Task AuthenticateReturnsNullIfPasswordIsIncorrect()
    {
        var fixture = AuthenticateFixture.ForPasswordIncorrect();

        Assert.Null(await fixture.Authenticate());
    }

    private sealed class AuthenticateFixture : AuthenticationManagerFixture
    {
        private const string Password = "user-password";
        private const string HashedPassword = "hashed-password";

        private AuthenticateFixture(User? user)
        {
            this.User = user;

            this.MockUserDataProvider
                .Setup(x => x.FindUserByEmail(this.MySqlConnection, this.SuppliedEmail))
                .ReturnsAsync(user);
        }

        public string SuppliedEmail { get; } = "supplied-email@example.com";

        public User? User { get; }

        public static AuthenticateFixture ForSuccess() =>
            ForPasswordVerificationResult(PasswordVerificationResult.Success);

        public static AuthenticateFixture ForEmailNotFound() => new(null);

        public static AuthenticateFixture ForUserHasNoPassword() =>
            new(new ModelFactory().BuildUser() with { HashedPassword = null });

        public static AuthenticateFixture ForPasswordIncorrect() =>
            ForPasswordVerificationResult(PasswordVerificationResult.Failed);

        public Task<User?> Authenticate() =>
            this.AuthenticationManager.Authenticate(this.SuppliedEmail, Password);

        private static AuthenticateFixture ForPasswordVerificationResult(
            PasswordVerificationResult result)
        {
            var user = new ModelFactory().BuildUser() with { HashedPassword = HashedPassword };

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
        var fixture = ChangePasswordFixture.ForUserHasNoPassword();

        try
        {
            await fixture.ChangePassword();
        }
        catch (InvalidOperationException)
        {
        }

        fixture.AssertAuthenticationEventLogged("password_change_failure:no_password_set", fixture.User.Id);
    }

    [Fact]
    public async Task ChangePasswordThrowsIfUserHasNoPassword()
    {
        var fixture = ChangePasswordFixture.ForUserHasNoPassword();

        await Assert.ThrowsAsync<InvalidOperationException>(fixture.ChangePassword);
    }

    [Fact]
    public async Task ChangePasswordLogsIfCurrentPasswordDoesNotMatch()
    {
        var fixture = ChangePasswordFixture.ForPasswordDoesNotMatch();

        await fixture.ChangePassword();

        fixture.Logger.AssertSingleEntry(
            LogLevel.Information,
            $"Password change denied for user {fixture.User.Id} ({fixture.User.Email}); current password is incorrect");

        fixture.AssertAuthenticationEventLogged(
            "password_change_failure:incorrect_password", fixture.User.Id);
    }

    [Fact]
    public async Task ChangePasswordReturnsFalseIfCurrentPasswordDoesNotMatch()
    {
        var fixture = ChangePasswordFixture.ForPasswordDoesNotMatch();

        Assert.False(await fixture.ChangePassword());
    }

    [Fact]
    public async Task ChangePasswordDoesNotChangePasswordIfCurrentPasswordDoesNotMatch()
    {
        var fixture = ChangePasswordFixture.ForPasswordDoesNotMatch();

        await fixture.ChangePassword();

        fixture.MockPasswordHasher.Verify(
            x => x.HashPassword(fixture.User, fixture.NewPassword), Times.Never);
    }

    [Fact]
    public async Task ChangePasswordUpdatesUserOnSuccess()
    {
        var fixture = ChangePasswordFixture.ForSuccess();

        await fixture.ChangePassword();

        fixture.MockUserDataProvider.Verify(x => x.UpdatePassword(
            fixture.MySqlConnection,
            fixture.User.Id,
            fixture.HashedNewPassword,
            fixture.NewSecurityStamp));
    }

    [Fact]
    public async Task ChangePasswordDeletesPasswordResetTokensOnSuccess()
    {
        var fixture = ChangePasswordFixture.ForSuccess();

        await fixture.ChangePassword();

        fixture.MockPasswordResetTokenDataProvider.Verify(
            x => x.DeleteTokensForUser(fixture.MySqlConnection, fixture.User.Id));
    }

    [Fact]
    public async Task ChangePasswordSendsPasswordChangeNotificationOnSuccess()
    {
        var fixture = ChangePasswordFixture.ForSuccess();

        await fixture.ChangePassword();

        fixture.MockAuthenticationMailer.Verify(
            x => x.SendPasswordChangeNotification(fixture.User.Email));
    }

    [Fact]
    public async Task ChangePasswordLogsOnSuccess()
    {
        var fixture = ChangePasswordFixture.ForSuccess();

        await fixture.ChangePassword();

        fixture.Logger.AssertSingleEntry(
            LogLevel.Information,
            $"Password successfully changed for user {fixture.User.Id} ({fixture.User.Email})");

        fixture.AssertAuthenticationEventLogged("password_change_success", fixture.User.Id);
    }

    [Fact]
    public async Task ChangePasswordSignsInUpdatedPrincipalOnSuccess()
    {
        var fixture = ChangePasswordFixture.ForSuccess();

        await fixture.ChangePassword();

        fixture.MockAuthenticationService.Verify(x => x.SignInAsync(fixture.HttpContext,
                CookieAuthenticationDefaults.AuthenticationScheme, fixture.UpdatedPrincipal, null));
    }

    [Fact]
    public async Task ChangePasswordReturnsTrueOnSuccess()
    {
        var fixture = ChangePasswordFixture.ForSuccess();

        Assert.True(await fixture.ChangePassword());
    }

    private sealed class ChangePasswordFixture : AuthenticationManagerFixture
    {
        private const string CurrentPassword = "current-password";
        private const string HashedCurrentPassword = "hashed-current-password";

        private ChangePasswordFixture(string? hashedPassword)
        {
            this.User = new ModelFactory().BuildUser() with { HashedPassword = hashedPassword };

            this.HttpContext.SetCurrentUser(this.User);

            this.MockPasswordHasher
                .Setup(x => x.HashPassword(this.User, this.NewPassword))
                .Returns(this.HashedNewPassword);

            this.MockRandomTokenGenerator
                .Setup(x => x.Generate(2))
                .Returns(this.NewSecurityStamp);

            var userForUpdatedPrincipal = this.User with { SecurityStamp = NewSecurityStamp };

            this.MockUserPrincipalFactory
                .Setup(x => x.Create(
                    userForUpdatedPrincipal, CookieAuthenticationDefaults.AuthenticationScheme))
                .Returns(this.UpdatedPrincipal);
        }

        public DefaultHttpContext HttpContext { get; } = new();

        public User User { get; }

        public string NewPassword { get; } = "new-password";

        public string HashedNewPassword { get; } = "hashed-new-password";

        public string NewSecurityStamp { get; } = "new-security-stamp";

        public ClaimsPrincipal UpdatedPrincipal { get; } = new();

        public static ChangePasswordFixture ForUserHasNoPassword() => new(null);

        public static ChangePasswordFixture ForPasswordDoesNotMatch() =>
            ForPasswordVerificationResult(PasswordVerificationResult.Failed);

        public static ChangePasswordFixture ForSuccess() =>
            ForPasswordVerificationResult(PasswordVerificationResult.Success);

        public Task<bool> ChangePassword() => this.AuthenticationManager.ChangePassword(
            this.HttpContext, CurrentPassword, this.NewPassword);

        private static ChangePasswordFixture ForPasswordVerificationResult(
            PasswordVerificationResult result)
        {
            var fixture = new ChangePasswordFixture(HashedCurrentPassword);

            fixture.MockPasswordHasher
                .Setup(x => x.VerifyHashedPassword(
                    fixture.User, HashedCurrentPassword, CurrentPassword))
                .Returns(result);

            return fixture;
        }
    }

    #endregion

    #region PasswordResetTokenIsValid

    [Fact]
    public async Task PasswordResetTokenIsValidDeletesExpiredTokens()
    {
        var fixture = PasswordResetTokenFixture.ForValidToken();

        await fixture.PasswordResetTokenIsValid();

        fixture.MockPasswordResetTokenDataProvider.Verify(
            x => x.DeleteExpiredTokens(fixture.MySqlConnection, fixture.UtcNow.AddDays(-1)));
    }

    [Fact]
    public async Task PasswordResetTokenIsValidLogsIfValid()
    {
        var fixture = PasswordResetTokenFixture.ForValidToken();

        await fixture.PasswordResetTokenIsValid();

        fixture.Logger.AssertSingleEntry(
            LogLevel.Debug,
            $"Password reset token '{fixture.RedactedToken}' is valid and belongs to user {fixture.UserId}");
    }

    [Fact]
    public async Task PasswordResetTokenIsValidReturnsTrueIfValid()
    {
        var fixture = PasswordResetTokenFixture.ForValidToken();

        Assert.True(await fixture.PasswordResetTokenIsValid());
    }

    [Fact]
    public async Task PasswordResetTokenIsValidLogsIfInvalid()
    {
        var fixture = PasswordResetTokenFixture.ForInvalidToken();

        await fixture.PasswordResetTokenIsValid();

        fixture.Logger.AssertSingleEntry(
            LogLevel.Debug,
            $"Password reset token '{fixture.RedactedToken}' is no longer valid");

        fixture.AssertAuthenticationEventLogged("password_reset_failure:invalid_token");
    }

    [Fact]
    public async Task PasswordResetTokenIsValidReturnsFalseIfInvalid()
    {
        var fixture = PasswordResetTokenFixture.ForInvalidToken();

        await fixture.PasswordResetTokenIsValid();

        Assert.False(await fixture.PasswordResetTokenIsValid());
    }

    private sealed class PasswordResetTokenFixture : AuthenticationManagerFixture
    {
        private const string Token = "password-reset-token";

        private PasswordResetTokenFixture(long? userId)
        {
            this.UserId = userId;
            this.SetupGetUserIdForToken(Token, userId);
        }

        public string RedactedToken { get; } = "passwo…";

        public long? UserId { get; }

        public static PasswordResetTokenFixture ForValidToken() => new(43);

        public static PasswordResetTokenFixture ForInvalidToken() => new(null);

        public Task<bool> PasswordResetTokenIsValid() =>
            this.AuthenticationManager.PasswordResetTokenIsValid(Token);
    }

    #endregion

    #region ResetPassword

    [Fact]
    public async Task ResetPasswordDeletesExpiredPasswordResetTokens()
    {
        var fixture = new ResetPasswordFixture();

        fixture.SetupSuccess();

        await fixture.ResetPassword();

        fixture.MockPasswordResetTokenDataProvider.Verify(
            x => x.DeleteExpiredTokens(fixture.MySqlConnection, fixture.UtcNow.AddDays(-1)));
    }

    [Fact]
    public async Task ResetPasswordLogsIfTokenIsInvalid()
    {
        var fixture = new ResetPasswordFixture();

        fixture.SetupInvalidToken();

        try
        {
            await fixture.ResetPassword();
        }
        catch (InvalidTokenException)
        {
        }

        fixture.Logger.AssertSingleEntry(
            LogLevel.Information,
            $"Unable to reset password; password reset token {fixture.RedactedToken} is invalid");

        fixture.AssertAuthenticationEventLogged("password_reset_failure:invalid_token");
    }

    [Fact]
    public async Task ResetPasswordThrowsIfTokenIsInvalid()
    {
        var fixture = new ResetPasswordFixture();

        fixture.SetupInvalidToken();

        await Assert.ThrowsAsync<InvalidTokenException>(fixture.ResetPassword);
    }

    [Fact]
    public async Task ResetPasswordUpdatesUserOnSuccess()
    {
        var fixture = new ResetPasswordFixture();

        fixture.SetupSuccess();

        await fixture.ResetPassword();

        fixture.MockUserDataProvider.Verify(x => x.UpdatePassword(
            fixture.MySqlConnection,
            fixture.User.Id,
            fixture.NewHashedPassword,
            fixture.NewSecurityStamp));
    }

    [Fact]
    public async Task ResetPasswordDeletesPasswordResetTokensOnSuccess()
    {
        var fixture = new ResetPasswordFixture();

        fixture.SetupSuccess();

        await fixture.ResetPassword();

        fixture.MockPasswordResetTokenDataProvider.Verify(
            x => x.DeleteTokensForUser(fixture.MySqlConnection, fixture.User.Id));
    }

    [Fact]
    public async Task ResetPasswordSendsPasswordChangeNotificationOnSuccess()
    {
        var fixture = new ResetPasswordFixture();

        fixture.SetupSuccess();

        await fixture.ResetPassword();

        fixture.MockAuthenticationMailer.Verify(
            x => x.SendPasswordChangeNotification(fixture.User.Email));
    }

    [Fact]
    public async Task ResetPasswordLogsOnSuccess()
    {
        var fixture = new ResetPasswordFixture();

        fixture.SetupSuccess();

        await fixture.ResetPassword();

        fixture.Logger.AssertSingleEntry(
            LogLevel.Information,
            $"Password reset for user {fixture.User.Id} using token {fixture.RedactedToken}");

        fixture.AssertAuthenticationEventLogged("password_reset_success", fixture.User.Id);
    }

    [Fact]
    public async Task ResetPasswordReturnsUserWithNewSecurityStampOnSuccess()
    {
        var fixture = new ResetPasswordFixture();

        fixture.SetupSuccess();

        var actual = await fixture.ResetPassword();

        Assert.Equal(fixture.User with { SecurityStamp = fixture.NewSecurityStamp }, actual);
    }

    private sealed class ResetPasswordFixture : AuthenticationManagerFixture
    {
        private const string NewPassword = "new-password";
        private const string Token = "password-reset-token";

        public string RedactedToken { get; } = "passwo…";

        public User User { get; } = new ModelFactory().BuildUser();

        public string NewHashedPassword { get; } = "new-hashed-password";

        public string NewSecurityStamp { get; } = "new-security-stamp";

        public void SetupInvalidToken() => this.SetupGetUserIdForToken(Token, null);

        public void SetupSuccess()
        {
            this.SetupGetUserIdForToken(Token, this.User.Id);
            this.SetupGetUser(this.User.Id, this.User);
            this.MockPasswordHasher
                .Setup(x => x.HashPassword(this.User, NewPassword))
                .Returns(this.NewHashedPassword);

            this.MockRandomTokenGenerator
                .Setup(x => x.Generate(2))
                .Returns(this.NewSecurityStamp);
        }

        public Task<User> ResetPassword() =>
            this.AuthenticationManager.ResetPassword(Token, NewPassword);
    }

    #endregion

    #region SendPasswordResetLink

    [Fact]
    public async Task SendPasswordResetLinkInsertsPasswordResetTokenOnSuccess()
    {
        var fixture = SendPasswordResetLinkFixture.ForSuccess();

        await fixture.SendPasswordResetLink();

        fixture.MockPasswordResetTokenDataProvider.Verify(x => x.InsertToken(
            fixture.MySqlConnection, fixture.User!.Id, fixture.Token));
    }

    [Fact]
    public async Task SendPasswordResetLinkSendsLinkToUserOnSuccess()
    {
        var fixture = SendPasswordResetLinkFixture.ForSuccess();

        await fixture.SendPasswordResetLink();

        fixture.MockAuthenticationMailer.Verify(
            x => x.SendPasswordResetLink(fixture.User!.Email, fixture.Link));
    }

    [Fact]
    public async Task SendPasswordResetLinkLogsOnSuccess()
    {
        var fixture = SendPasswordResetLinkFixture.ForSuccess();

        await fixture.SendPasswordResetLink();

        var user = fixture.User!;

        fixture.Logger.AssertSingleEntry(
            LogLevel.Information, $"Password reset link sent to user {user.Id} ({user.Email})");

        fixture.AssertAuthenticationEventLogged("password_reset_link_sent", user.Id, user.Email);
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
    public async Task SendPasswordResetLinkLogsIfEmailIsUnrecognized()
    {
        var fixture = SendPasswordResetLinkFixture.ForUnrecognizedEmail();

        await fixture.SendPasswordResetLink();

        fixture.Logger.AssertSingleEntry(
            LogLevel.Information,
            $"Unable to send password reset link; No user with email {fixture.SuppliedEmail}");

        fixture.AssertAuthenticationEventLogged(
            "password_reset_failure:unrecognized_email", null, fixture.SuppliedEmail);
    }

    private sealed class SendPasswordResetLinkFixture : AuthenticationManagerFixture
    {
        private SendPasswordResetLinkFixture(User? user)
        {
            this.User = user;

            this.MockUrlHelperFactory
                .Setup(x => x.GetUrlHelper(this.ActionContext))
                .Returns(this.MockUrlHelper.Object);

            this.MockUserDataProvider
                .Setup(x => x.FindUserByEmail(this.MySqlConnection, this.SuppliedEmail))
                .ReturnsAsync(user);
        }

        public Mock<IUrlHelper> MockUrlHelper { get; } = new();

        public ActionContext ActionContext { get; } = new();

        public string SuppliedEmail { get; } = "supplied-email@example.com";

        public User? User { get; }

        public string Token { get; } = "password-reset-token";

        public string Link { get; } = "https://example.com/reset-password/token";

        public static SendPasswordResetLinkFixture ForSuccess()
        {
            var fixture = new SendPasswordResetLinkFixture(new ModelFactory().BuildUser());

            fixture.MockRandomTokenGenerator
                .Setup(x => x.Generate(12))
                .Returns(fixture.Token);

            fixture.MockUrlHelper
                .Setup(x => x.Link(
                    "ResetPassword",
                    It.Is<object>(o => fixture.Token.Equals(new RouteValueDictionary(o)["token"]))))
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

        await fixture.SignIn();

        fixture.MockAuthenticationService.Verify(x => x.SignInAsync(fixture.HttpContext,
                CookieAuthenticationDefaults.AuthenticationScheme, fixture.UserPrincipal, null));
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

        fixture.Logger.AssertSingleEntry(
            LogLevel.Information, $"User {fixture.User.Id} ({fixture.User.Email}) signed in");

        fixture.AssertAuthenticationEventLogged("sign_in", fixture.User.Id);
    }

    private sealed class SignInFixture : AuthenticationManagerFixture
    {
        public SignInFixture() =>
            this.MockUserPrincipalFactory
                .Setup(x => x.Create(this.User, CookieAuthenticationDefaults.AuthenticationScheme))
                .Returns(this.UserPrincipal);

        public DefaultHttpContext HttpContext { get; } = new();

        public User User { get; } = new ModelFactory().BuildUser();

        public ClaimsPrincipal UserPrincipal { get; } = new();

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
    public async Task SignOutLogsIfUserPreviouslySignedIn()
    {
        var fixture = SignOutFixture.ForUserSignedIn();

        await fixture.SignOut();

        fixture.Logger.AssertSingleEntry(
            LogLevel.Information, $"User {fixture.UserId} ({fixture.Email}) signed out");

        fixture.AssertAuthenticationEventLogged("sign_out", fixture.UserId);
    }

    [Fact]
    public async Task SignOutDoesNotLogsIfNoUserPreviouslySignedIn()
    {
        var fixture = SignOutFixture.ForNoUserSignedIn();

        await fixture.SignOut();

        fixture.MockAuthenticationEventDataProvider.Verify(
            x => x.LogEvent(fixture.MySqlConnection, "sign_out", null, null), Times.Never);
    }

    private sealed class SignOutFixture : AuthenticationManagerFixture
    {
        private SignOutFixture(long? userId) => this.UserId = userId;

        public DefaultHttpContext HttpContext { get; } = new();

        public long? UserId { get; }

        public string Email { get; } = "sample@example.com";

        public static SignOutFixture ForNoUserSignedIn() => new(null);

        public static SignOutFixture ForUserSignedIn()
        {
            var fixture = new SignOutFixture(76);

            var claims = new Claim[]
            {
                new(ClaimTypes.NameIdentifier, "76"),
                new(ClaimTypes.Email, fixture.Email),
            };

            fixture.HttpContext.User = new ClaimsPrincipal(
                new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));

            return fixture;
        }

        public Task SignOut() => this.AuthenticationManager.SignOut(this.HttpContext);
    }

    #endregion

    #region ValidatePrincipal

    [Fact]
    public async Task ValidatePrincipalDoesNotRejectPrincipalWhenUnauthenticated()
    {
        var fixture = ValidatePrincipalFixture.ForUnauthenticated();

        await fixture.ValidatePrincipal();

        Assert.Same(fixture.InitialPrincipal, fixture.CookieContext.Principal);
    }

    [Fact]
    public async Task ValidatePrincipalSetsCurrentUserWhenStampIsCorrect()
    {
        var fixture = ValidatePrincipalFixture.ForCorrectStamp();

        await fixture.ValidatePrincipal();

        Assert.Equal(fixture.User, fixture.CookieContext.HttpContext.GetCurrentUser());
    }

    [Fact]
    public async Task ValidatePrincipalDoesNotRejectPrincipalWhenStampIsCorrect()
    {
        var fixture = ValidatePrincipalFixture.ForCorrectStamp();

        await fixture.ValidatePrincipal();

        Assert.Same(fixture.InitialPrincipal, fixture.CookieContext.Principal);
    }

    [Fact]
    public async Task ValidatePrincipalLogsWhenStampIsCorrect()
    {
        var fixture = ValidatePrincipalFixture.ForCorrectStamp();

        await fixture.ValidatePrincipal();

        fixture.Logger.AssertSingleEntry(
            LogLevel.Debug,
            $"Principal successfully validated for user {fixture.User.Id} ({fixture.User.Email})");
    }

    [Fact]
    public async Task ValidatePrincipalLogsWhenStampIsIncorrect()
    {
        var fixture = ValidatePrincipalFixture.ForIncorrectStamp();

        await fixture.ValidatePrincipal();

        fixture.Logger.AssertSingleEntry(
            LogLevel.Information,
            $"Incorrect security stamp for user {fixture.User.Id} ({fixture.User.Email})");
    }

    [Fact]
    public async Task ValidatePrincipalRejectsPrincipalWhenStampIsIncorrect()
    {
        var fixture = ValidatePrincipalFixture.ForIncorrectStamp();

        await fixture.ValidatePrincipal();

        Assert.Null(fixture.CookieContext.Principal);
    }

    [Fact]
    public async Task ValidatePrincipalSignsUserOutWhenStampIsIncorrect()
    {
        var fixture = ValidatePrincipalFixture.ForIncorrectStamp();

        await fixture.ValidatePrincipal();

        fixture.MockAuthenticationService.Verify(
            x => x.SignOutAsync(
                fixture.CookieContext.HttpContext,
                CookieAuthenticationDefaults.AuthenticationScheme,
                null));
    }

    private sealed class ValidatePrincipalFixture : AuthenticationManagerFixture
    {
        private const long UserId = 34;
        private const string UserSecurityStamp = "user-security-stamp";

        private ValidatePrincipalFixture(params Claim[] claims)
        {
            this.InitialPrincipal = new(new ClaimsIdentity(claims));

            var scheme = new AuthenticationScheme(
                CookieAuthenticationDefaults.AuthenticationScheme,
                null,
                typeof(CookieAuthenticationHandler));
            var ticket = new AuthenticationTicket(
                this.InitialPrincipal,
                CookieAuthenticationDefaults.AuthenticationScheme);

            this.CookieContext = new(new DefaultHttpContext(), scheme, new(), ticket);
        }

        public ClaimsPrincipal InitialPrincipal { get; }

        public CookieValidatePrincipalContext CookieContext { get; }

        public User User { get; } = new ModelFactory().BuildUser() with
        {
            Id = UserId,
            SecurityStamp = UserSecurityStamp,
        };

        public static ValidatePrincipalFixture ForUnauthenticated() => new();

        public static ValidatePrincipalFixture ForCorrectStamp() =>
            ForAuthenticated(UserSecurityStamp);

        public static ValidatePrincipalFixture ForIncorrectStamp() =>
            ForAuthenticated("stale-security-stamp");

        public Task ValidatePrincipal() =>
            this.AuthenticationManager.ValidatePrincipal(this.CookieContext);

        private static ValidatePrincipalFixture ForAuthenticated(string principalSecurityStamp)
        {
            var fixture = new ValidatePrincipalFixture(
                new(ClaimTypes.NameIdentifier, UserId.ToString(CultureInfo.InvariantCulture)),
                new(CustomClaimTypes.SecurityStamp, principalSecurityStamp));

            fixture.SetupGetUser(UserId, fixture.User);

            return fixture;
        }
    }

    #endregion

    private class AuthenticationManagerFixture
    {
        public AuthenticationManagerFixture()
        {
            var clock = Mock.Of<IClock>(x => x.UtcNow == this.UtcNow);
            var mySqlConnectionSource = Mock.Of<IMySqlConnectionSource>(
                x => x.OpenConnection() == Task.FromResult(this.MySqlConnection));

            this.AuthenticationManager = new(
                this.MockAuthenticationEventDataProvider.Object,
                this.MockAuthenticationMailer.Object,
                this.MockAuthenticationService.Object,
                clock,
                mySqlConnectionSource,
                this.Logger,
                this.MockPasswordHasher.Object,
                this.MockPasswordResetTokenDataProvider.Object,
                this.MockRandomTokenGenerator.Object,
                this.MockUrlHelperFactory.Object,
                this.MockUserDataProvider.Object,
                this.MockUserPrincipalFactory.Object);
        }

        public Mock<IAuthenticationEventDataProvider> MockAuthenticationEventDataProvider { get; } = new();

        public AuthenticationManager AuthenticationManager { get; }

        public ListLogger<AuthenticationManager> Logger { get; } = new();

        public MySqlConnection MySqlConnection { get; } = new();

        public Mock<IAuthenticationMailer> MockAuthenticationMailer { get; } = new();

        public Mock<IAuthenticationService> MockAuthenticationService { get; } = new();

        public Mock<IPasswordHasher<User>> MockPasswordHasher { get; } = new();

        public Mock<IPasswordResetTokenDataProvider> MockPasswordResetTokenDataProvider { get; } = new();

        public Mock<IRandomTokenGenerator> MockRandomTokenGenerator { get; } = new();

        public Mock<IUrlHelperFactory> MockUrlHelperFactory { get; } = new();

        public Mock<IUserDataProvider> MockUserDataProvider { get; } = new();

        public Mock<IUserPrincipalFactory> MockUserPrincipalFactory { get; } = new();

        public DateTime UtcNow { get; } = new(2000, 1, 2, 3, 4, 5, DateTimeKind.Utc);

        public void SetupGetUser(long id, User user) =>
            this.MockUserDataProvider
                .Setup(x => x.GetUser(this.MySqlConnection, id))
                .ReturnsAsync(user);

        public void SetupGetUserIdForToken(string token, long? userId) =>
            this.MockPasswordResetTokenDataProvider
                .Setup(x => x.GetUserIdForToken(this.MySqlConnection, token))
                .ReturnsAsync(userId);

        public void AssertAuthenticationEventLogged(
            string eventName, long? userId = null, string? email = null) =>
            this.MockAuthenticationEventDataProvider.Verify(x => x.LogEvent(
                this.MySqlConnection, eventName, userId, email));
    }
}
