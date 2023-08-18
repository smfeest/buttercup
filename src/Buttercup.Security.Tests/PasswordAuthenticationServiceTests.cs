using Buttercup.DataAccess;
using Buttercup.EntityModel;
using Buttercup.TestUtils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Buttercup.Security;

public sealed class PasswordAuthenticationServiceTests
{
    #region Authenticate

    [Fact]
    public async Task AuthenticateLogsOnSuccess()
    {
        using var fixture = AuthenticateFixture.ForSuccess();

        await fixture.Authenticate();

        var user = fixture.User!;

        Assert.Contains(
            fixture.Logger.Entries,
            entry =>
                entry.LogLevel == LogLevel.Information &&
                entry.Message == $"User {user.Id} ({user.Email}) successfully authenticated");

        fixture.AssertAuthenticationEventLogged(
            "authentication_success", user.Id, fixture.SuppliedEmail);
    }

    [Fact]
    public async Task AuthenticateReturnsUserOnSuccess()
    {
        using var fixture = AuthenticateFixture.ForSuccess();

        var actual = await fixture.Authenticate();

        Assert.Equal(fixture.User, actual);
    }

    [Fact]
    public async Task AuthenticateLogsIfEmailIsUnrecognized()
    {
        using var fixture = AuthenticateFixture.ForEmailNotFound();

        await fixture.Authenticate();

        Assert.Contains(
            fixture.Logger.Entries,
            entry =>
                entry.LogLevel == LogLevel.Information &&
                entry.Message == $"Authentication failed; no user with email {fixture.SuppliedEmail}");

        fixture.AssertAuthenticationEventLogged(
            "authentication_failure:unrecognized_email", null, fixture.SuppliedEmail);
    }

    [Fact]
    public async Task AuthenticateReturnsNullIfEmailIsUnrecognized()
    {
        using var fixture = AuthenticateFixture.ForEmailNotFound();

        Assert.Null(await fixture.Authenticate());
    }

    [Fact]
    public async Task AuthenticateLogsIfUserHasNoPassword()
    {
        using var fixture = AuthenticateFixture.ForUserHasNoPassword();

        await fixture.Authenticate();

        var user = fixture.User!;

        Assert.Contains(
            fixture.Logger.Entries,
            entry =>
                entry.LogLevel == LogLevel.Information &&
                entry.Message == $"Authentication failed; no password set for user {user.Id} ({user.Email})");

        fixture.AssertAuthenticationEventLogged(
            "authentication_failure:no_password_set", user.Id, fixture.SuppliedEmail);
    }

    [Fact]
    public async Task AuthenticateReturnsNullIfUserHasNoPassword()
    {
        using var fixture = AuthenticateFixture.ForUserHasNoPassword();

        Assert.Null(await fixture.Authenticate());
    }

    [Fact]
    public async Task AuthenticateLogsIfPasswordIsIncorrect()
    {
        using var fixture = AuthenticateFixture.ForPasswordIncorrect();

        await fixture.Authenticate();

        var user = fixture.User!;

        Assert.Contains(
            fixture.Logger.Entries,
            entry =>
                entry.LogLevel == LogLevel.Information &&
                entry.Message == $"Authentication failed; incorrect password for user {user.Id} ({user.Email})");

        fixture.AssertAuthenticationEventLogged(
            "authentication_failure:incorrect_password", user.Id, fixture.SuppliedEmail);
    }

    [Fact]
    public async Task AuthenticateReturnsNullIfPasswordIsIncorrect()
    {
        using var fixture = AuthenticateFixture.ForPasswordIncorrect();

        Assert.Null(await fixture.Authenticate());
    }

    private sealed class AuthenticateFixture : PasswordAuthenticationServiceFixture
    {
        private const string Password = "user-password";
        private const string HashedPassword = "hashed-password";

        private AuthenticateFixture(User? user)
        {
            this.User = user;

            this.MockUserDataProvider
                .Setup(
                    x => x.FindUserByEmail(this.DbContextFactory.FakeDbContext, this.SuppliedEmail))
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
            this.PasswordAuthenticationService.Authenticate(this.SuppliedEmail, Password);

        private static AuthenticateFixture ForPasswordVerificationResult(
            PasswordVerificationResult result)
        {
            var user = new ModelFactory().BuildUser() with { HashedPassword = HashedPassword };

            using var fixture = new AuthenticateFixture(user);

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
        using var fixture = ChangePasswordFixture.ForUserHasNoPassword();

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
        using var fixture = ChangePasswordFixture.ForUserHasNoPassword();

        await Assert.ThrowsAsync<InvalidOperationException>(fixture.ChangePassword);
    }

    [Fact]
    public async Task ChangePasswordLogsIfCurrentPasswordDoesNotMatch()
    {
        using var fixture = ChangePasswordFixture.ForPasswordDoesNotMatch();

        await fixture.ChangePassword();

        Assert.Contains(
            fixture.Logger.Entries,
            entry =>
                entry.LogLevel == LogLevel.Information &&
                entry.Message == $"Password change denied for user {fixture.User.Id} ({fixture.User.Email}); current password is incorrect");

        fixture.AssertAuthenticationEventLogged(
            "password_change_failure:incorrect_password", fixture.User.Id);
    }

    [Fact]
    public async Task ChangePasswordReturnsFalseIfCurrentPasswordDoesNotMatch()
    {
        using var fixture = ChangePasswordFixture.ForPasswordDoesNotMatch();

        Assert.False(await fixture.ChangePassword());
    }

    [Fact]
    public async Task ChangePasswordDoesNotChangePasswordIfCurrentPasswordDoesNotMatch()
    {
        using var fixture = ChangePasswordFixture.ForPasswordDoesNotMatch();

        await fixture.ChangePassword();

        fixture.MockPasswordHasher.Verify(
            x => x.HashPassword(fixture.User, fixture.NewPassword), Times.Never);
    }

    [Fact]
    public async Task ChangePasswordUpdatesUserOnSuccess()
    {
        using var fixture = ChangePasswordFixture.ForSuccess();

        await fixture.ChangePassword();

        fixture.MockUserDataProvider.Verify(x => x.UpdatePassword(
            fixture.DbContextFactory.FakeDbContext,
            fixture.User.Id,
            fixture.HashedNewPassword,
            fixture.NewSecurityStamp));
    }

    [Fact]
    public async Task ChangePasswordDeletesPasswordResetTokensOnSuccess()
    {
        using var fixture = ChangePasswordFixture.ForSuccess();

        await fixture.ChangePassword();

        fixture.MockPasswordResetTokenDataProvider.Verify(
            x => x.DeleteTokensForUser(fixture.DbContextFactory.FakeDbContext, fixture.User.Id));
    }

    [Fact]
    public async Task ChangePasswordSendsPasswordChangeNotificationOnSuccess()
    {
        using var fixture = ChangePasswordFixture.ForSuccess();

        await fixture.ChangePassword();

        fixture.MockAuthenticationMailer.Verify(
            x => x.SendPasswordChangeNotification(fixture.User.Email));
    }

    [Fact]
    public async Task ChangePasswordLogsOnSuccess()
    {
        using var fixture = ChangePasswordFixture.ForSuccess();

        await fixture.ChangePassword();

        Assert.Contains(
            fixture.Logger.Entries,
            entry =>
                entry.LogLevel == LogLevel.Information &&
                entry.Message == $"Password successfully changed for user {fixture.User.Id} ({fixture.User.Email})");

        fixture.AssertAuthenticationEventLogged("password_change_success", fixture.User.Id);
    }

    [Fact]
    public async Task ChangePasswordReturnsTrueOnSuccess()
    {
        using var fixture = ChangePasswordFixture.ForSuccess();

        Assert.True(await fixture.ChangePassword());
    }

    private sealed class ChangePasswordFixture : PasswordAuthenticationServiceFixture
    {
        private const string CurrentPassword = "current-password";
        private const string HashedCurrentPassword = "hashed-current-password";

        private ChangePasswordFixture(string? hashedPassword)
        {
            this.User = new ModelFactory().BuildUser() with { HashedPassword = hashedPassword };

            this.SetupGetUser(this.User.Id, this.User);

            this.MockPasswordHasher
                .Setup(x => x.HashPassword(this.User, this.NewPassword))
                .Returns(this.HashedNewPassword);

            this.MockRandomTokenGenerator
                .Setup(x => x.Generate(2))
                .Returns(this.NewSecurityStamp);
        }

        public User User { get; }

        public string NewPassword { get; } = "new-password";

        public string HashedNewPassword { get; } = "hashed-new-password";

        public string NewSecurityStamp { get; } = "new-security-stamp";

        public static ChangePasswordFixture ForUserHasNoPassword() => new(null);

        public static ChangePasswordFixture ForPasswordDoesNotMatch() =>
            ForPasswordVerificationResult(PasswordVerificationResult.Failed);

        public static ChangePasswordFixture ForSuccess() =>
            ForPasswordVerificationResult(PasswordVerificationResult.Success);

        public Task<bool> ChangePassword() => this.PasswordAuthenticationService.ChangePassword(
            this.User.Id, CurrentPassword, this.NewPassword);

        private static ChangePasswordFixture ForPasswordVerificationResult(
            PasswordVerificationResult result)
        {
            using var fixture = new ChangePasswordFixture(HashedCurrentPassword);

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
        using var fixture = PasswordResetTokenFixture.ForValidToken();

        await fixture.PasswordResetTokenIsValid();

        fixture.MockPasswordResetTokenDataProvider.Verify(x => x.DeleteExpiredTokens(
            fixture.DbContextFactory.FakeDbContext, fixture.UtcNow.AddDays(-1)));
    }

    [Fact]
    public async Task PasswordResetTokenIsValidLogsIfValid()
    {
        using var fixture = PasswordResetTokenFixture.ForValidToken();

        await fixture.PasswordResetTokenIsValid();

        Assert.Contains(
            fixture.Logger.Entries,
            entry =>
                entry.LogLevel == LogLevel.Debug &&
                entry.Message == $"Password reset token '{fixture.RedactedToken}' is valid and belongs to user {fixture.UserId}");
    }

    [Fact]
    public async Task PasswordResetTokenIsValidReturnsTrueIfValid()
    {
        using var fixture = PasswordResetTokenFixture.ForValidToken();

        Assert.True(await fixture.PasswordResetTokenIsValid());
    }

    [Fact]
    public async Task PasswordResetTokenIsValidLogsIfInvalid()
    {
        using var fixture = PasswordResetTokenFixture.ForInvalidToken();

        await fixture.PasswordResetTokenIsValid();

        Assert.Contains(
            fixture.Logger.Entries,
            entry =>
                entry.LogLevel == LogLevel.Debug &&
                entry.Message == $"Password reset token '{fixture.RedactedToken}' is no longer valid");

        fixture.AssertAuthenticationEventLogged("password_reset_failure:invalid_token");
    }

    [Fact]
    public async Task PasswordResetTokenIsValidReturnsFalseIfInvalid()
    {
        using var fixture = PasswordResetTokenFixture.ForInvalidToken();

        await fixture.PasswordResetTokenIsValid();

        Assert.False(await fixture.PasswordResetTokenIsValid());
    }

    private sealed class PasswordResetTokenFixture : PasswordAuthenticationServiceFixture
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
            this.PasswordAuthenticationService.PasswordResetTokenIsValid(Token);
    }

    #endregion

    #region ResetPassword

    [Fact]
    public async Task ResetPasswordDeletesExpiredPasswordResetTokens()
    {
        using var fixture = new ResetPasswordFixture();

        fixture.SetupSuccess();

        await fixture.ResetPassword();

        fixture.MockPasswordResetTokenDataProvider.Verify(x => x.DeleteExpiredTokens(
            fixture.DbContextFactory.FakeDbContext, fixture.UtcNow.AddDays(-1)));
    }

    [Fact]
    public async Task ResetPasswordLogsIfTokenIsInvalid()
    {
        using var fixture = new ResetPasswordFixture();

        fixture.SetupInvalidToken();

        try
        {
            await fixture.ResetPassword();
        }
        catch (InvalidTokenException)
        {
        }

        Assert.Contains(
            fixture.Logger.Entries,
            entry =>
                entry.LogLevel == LogLevel.Information &&
                entry.Message == $"Unable to reset password; password reset token {fixture.RedactedToken} is invalid");

        fixture.AssertAuthenticationEventLogged("password_reset_failure:invalid_token");
    }

    [Fact]
    public async Task ResetPasswordThrowsIfTokenIsInvalid()
    {
        using var fixture = new ResetPasswordFixture();

        fixture.SetupInvalidToken();

        await Assert.ThrowsAsync<InvalidTokenException>(fixture.ResetPassword);
    }

    [Fact]
    public async Task ResetPasswordUpdatesUserOnSuccess()
    {
        using var fixture = new ResetPasswordFixture();

        fixture.SetupSuccess();

        await fixture.ResetPassword();

        fixture.MockUserDataProvider.Verify(x => x.UpdatePassword(
            fixture.DbContextFactory.FakeDbContext,
            fixture.User.Id,
            fixture.NewHashedPassword,
            fixture.NewSecurityStamp));
    }

    [Fact]
    public async Task ResetPasswordDeletesPasswordResetTokensOnSuccess()
    {
        using var fixture = new ResetPasswordFixture();

        fixture.SetupSuccess();

        await fixture.ResetPassword();

        fixture.MockPasswordResetTokenDataProvider.Verify(
            x => x.DeleteTokensForUser(fixture.DbContextFactory.FakeDbContext, fixture.User.Id));
    }

    [Fact]
    public async Task ResetPasswordSendsPasswordChangeNotificationOnSuccess()
    {
        using var fixture = new ResetPasswordFixture();

        fixture.SetupSuccess();

        await fixture.ResetPassword();

        fixture.MockAuthenticationMailer.Verify(
            x => x.SendPasswordChangeNotification(fixture.User.Email));
    }

    [Fact]
    public async Task ResetPasswordLogsOnSuccess()
    {
        using var fixture = new ResetPasswordFixture();

        fixture.SetupSuccess();

        await fixture.ResetPassword();

        Assert.Contains(
            fixture.Logger.Entries,
            entry =>
                entry.LogLevel == LogLevel.Information &&
                entry.Message == $"Password reset for user {fixture.User.Id} using token {fixture.RedactedToken}");

        fixture.AssertAuthenticationEventLogged("password_reset_success", fixture.User.Id);
    }

    [Fact]
    public async Task ResetPasswordReturnsUserWithNewSecurityStampOnSuccess()
    {
        using var fixture = new ResetPasswordFixture();

        fixture.SetupSuccess();

        var actual = await fixture.ResetPassword();

        Assert.Equal(fixture.User with { SecurityStamp = fixture.NewSecurityStamp }, actual);
    }

    private sealed class ResetPasswordFixture : PasswordAuthenticationServiceFixture
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
            this.PasswordAuthenticationService.ResetPassword(Token, NewPassword);
    }

    #endregion

    #region SendPasswordResetLink

    [Fact]
    public async Task SendPasswordResetLinkInsertsPasswordResetTokenOnSuccess()
    {
        using var fixture = SendPasswordResetLinkFixture.ForSuccess();

        await fixture.SendPasswordResetLink();

        fixture.MockPasswordResetTokenDataProvider.Verify(x => x.InsertToken(
            fixture.DbContextFactory.FakeDbContext, fixture.User!.Id, fixture.Token));
    }

    [Fact]
    public async Task SendPasswordResetLinkSendsLinkToUserOnSuccess()
    {
        using var fixture = SendPasswordResetLinkFixture.ForSuccess();

        await fixture.SendPasswordResetLink();

        fixture.MockAuthenticationMailer.Verify(
            x => x.SendPasswordResetLink(fixture.User!.Email, fixture.Link));
    }

    [Fact]
    public async Task SendPasswordResetLinkLogsOnSuccess()
    {
        using var fixture = SendPasswordResetLinkFixture.ForSuccess();

        await fixture.SendPasswordResetLink();

        var user = fixture.User!;

        Assert.Contains(
            fixture.Logger.Entries,
            entry =>
                entry.LogLevel == LogLevel.Information &&
                entry.Message == $"Password reset link sent to user {user.Id} ({user.Email})");

        fixture.AssertAuthenticationEventLogged("password_reset_link_sent", user.Id, user.Email);
    }

    [Fact]
    public async Task SendPasswordResetLinkDoesNotSendLinkIfEmailIsUnrecognized()
    {
        using var fixture = SendPasswordResetLinkFixture.ForUnrecognizedEmail();

        await fixture.SendPasswordResetLink();

        fixture.MockAuthenticationMailer.Verify(
            x => x.SendPasswordResetLink(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SendPasswordResetLinkLogsIfEmailIsUnrecognized()
    {
        using var fixture = SendPasswordResetLinkFixture.ForUnrecognizedEmail();

        await fixture.SendPasswordResetLink();

        Assert.Contains(
            fixture.Logger.Entries,
            entry =>
                entry.LogLevel == LogLevel.Information &&
                entry.Message == $"Unable to send password reset link; No user with email {fixture.SuppliedEmail}");

        fixture.AssertAuthenticationEventLogged(
            "password_reset_failure:unrecognized_email", null, fixture.SuppliedEmail);
    }

    private sealed class SendPasswordResetLinkFixture : PasswordAuthenticationServiceFixture
    {
        private SendPasswordResetLinkFixture(User? user)
        {
            this.User = user;

            this.MockUserDataProvider
                .Setup(
                    x => x.FindUserByEmail(this.DbContextFactory.FakeDbContext, this.SuppliedEmail))
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
            using var fixture = new SendPasswordResetLinkFixture(new ModelFactory().BuildUser());

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
            this.PasswordAuthenticationService.SendPasswordResetLink(
                this.SuppliedEmail, this.MockUrlHelper.Object);
    }

    #endregion

    private class PasswordAuthenticationServiceFixture : IDisposable
    {
        public PasswordAuthenticationServiceFixture()
        {
            var clock = Mock.Of<IClock>(x => x.UtcNow == this.UtcNow);

            this.PasswordAuthenticationService = new(
                this.MockAuthenticationEventDataProvider.Object,
                this.MockAuthenticationMailer.Object,
                clock,
                this.DbContextFactory,
                this.Logger,
                this.MockPasswordHasher.Object,
                this.MockPasswordResetTokenDataProvider.Object,
                this.MockRandomTokenGenerator.Object,
                this.MockUserDataProvider.Object);
        }

        public Mock<IAuthenticationEventDataProvider> MockAuthenticationEventDataProvider { get; } = new();

        public PasswordAuthenticationService PasswordAuthenticationService { get; }

        public FakeDbContextFactory DbContextFactory { get; } = new();

        public ListLogger<PasswordAuthenticationService> Logger { get; } = new();

        public Mock<IAuthenticationMailer> MockAuthenticationMailer { get; } = new();

        public Mock<IPasswordHasher<User>> MockPasswordHasher { get; } = new();

        public Mock<IPasswordResetTokenDataProvider> MockPasswordResetTokenDataProvider { get; } = new();

        public Mock<IRandomTokenGenerator> MockRandomTokenGenerator { get; } = new();

        public Mock<IUserDataProvider> MockUserDataProvider { get; } = new();

        public DateTime UtcNow { get; } = new(2000, 1, 2, 3, 4, 5, DateTimeKind.Utc);

        public void SetupGetUser(long id, User user) =>
            this.MockUserDataProvider
                .Setup(x => x.GetUser(this.DbContextFactory.FakeDbContext, id))
                .ReturnsAsync(user);

        public void SetupGetUserIdForToken(string token, long? userId) =>
            this.MockPasswordResetTokenDataProvider
                .Setup(x => x.GetUserIdForToken(this.DbContextFactory.FakeDbContext, token))
                .ReturnsAsync(userId);

        public void AssertAuthenticationEventLogged(
            string eventName, long? userId = null, string? email = null) =>
            this.MockAuthenticationEventDataProvider.Verify(x => x.LogEvent(
                this.DbContextFactory.FakeDbContext, eventName, userId, email));

        public void Dispose() => this.DbContextFactory.Dispose();
    }
}
