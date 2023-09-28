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

public sealed class PasswordAuthenticationServiceTests : IDisposable
{
    private readonly ModelFactory modelFactory = new();

    private readonly Mock<IAuthenticationMailer> authenticationMailerMock = new();
    private readonly StoppedClock clock = new();
    private readonly FakeDbContextFactory dbContextFactory = new();
    private readonly ListLogger<PasswordAuthenticationService> logger = new();
    private readonly Mock<IPasswordHasher<User>> passwordHasherMock = new();
    private readonly Mock<IPasswordResetTokenDataProvider> passwordResetTokenDataProviderMock =
        new();
    private readonly Mock<IRandomTokenGenerator> randomTokenGeneratorMock = new();
    private readonly Mock<ISecurityEventDataProvider> securityEventDataProviderMock = new();
    private readonly Mock<IUserDataProvider> userDataProviderMock = new();

    private readonly PasswordAuthenticationService passwordAuthenticationService;

    public PasswordAuthenticationServiceTests() =>
        this.passwordAuthenticationService = new(
            this.authenticationMailerMock.Object,
            this.clock,
            this.dbContextFactory,
            this.logger,
            this.passwordHasherMock.Object,
            this.passwordResetTokenDataProviderMock.Object,
            this.randomTokenGeneratorMock.Object,
            this.securityEventDataProviderMock.Object,
            this.userDataProviderMock.Object);

    public void Dispose() => this.dbContextFactory.Dispose();

    #region Authenticate

    [Fact]
    public async Task Authenticate_Success_LogsSuccess()
    {
        var values = this.SetupAuthenticate_Success();

        await this.passwordAuthenticationService.Authenticate(
            values.SuppliedEmail, values.SuppliedPassword);

        LogAssert.HasEntry(
            this.logger,
            LogLevel.Information,
            202,
            $"User {values.User.Id} ({values.User.Email}) successfully authenticated");
    }

    [Fact]
    public async Task Authenticate_Success_InsertsSecurityEvent()
    {
        var values = this.SetupAuthenticate_Success();

        await this.passwordAuthenticationService.Authenticate(
            values.SuppliedEmail, values.SuppliedPassword);

        this.AssertSecurityEventLogged("authentication_success", values.User.Id);
    }

    [Fact]
    public async Task Authenticate_Success_ReturnsUser()
    {
        var values = this.SetupAuthenticate_Success();

        var returnedUser = await this.passwordAuthenticationService.Authenticate(
            values.SuppliedEmail, values.SuppliedPassword);

        Assert.Equal(values.User, returnedUser);
    }

    [Fact]
    public async Task Authenticate_EmailUnrecognized_LogsUnrecognizedEmail()
    {
        var values = this.SetupAuthenticate_EmailUnrecognized();

        await this.passwordAuthenticationService.Authenticate(
            values.SuppliedEmail, values.SuppliedPassword);

        LogAssert.HasEntry(
            this.logger,
            LogLevel.Information,
            203,
            $"Authentication failed; no user with email {values.SuppliedEmail}");
    }

    [Fact]
    public async Task Authenticate_EmailUnrecognized_InsertsSecurityEvent()
    {
        var values = this.SetupAuthenticate_EmailUnrecognized();

        await this.passwordAuthenticationService.Authenticate(
            values.SuppliedEmail, values.SuppliedPassword);

        this.AssertSecurityEventLogged("authentication_failure:unrecognized_email");
    }

    [Fact]
    public async Task Authenticate_EmailUnrecognized_ReturnsNull()
    {
        var values = this.SetupAuthenticate_EmailUnrecognized();

        Assert.Null(
            await this.passwordAuthenticationService.Authenticate(
                values.SuppliedEmail, values.SuppliedPassword));
    }

    [Fact]
    public async Task Authenticate_UserHasNoPassword_LogsNoPassword()
    {
        var values = this.SetupAuthenticate_UserHasNoPassword();

        await this.passwordAuthenticationService.Authenticate(
            values.SuppliedEmail, values.SuppliedPassword);

        LogAssert.HasEntry(
            this.logger,
            LogLevel.Information,
            201,
            $"Authentication failed; no password set for user {values.User.Id} ({values.User.Email})");
    }

    [Fact]
    public async Task Authenticate_UserHasNoPassword_InsertsSecurityEvent()
    {
        var values = this.SetupAuthenticate_UserHasNoPassword();

        await this.passwordAuthenticationService.Authenticate(
            values.SuppliedEmail, values.SuppliedPassword);

        this.AssertSecurityEventLogged("authentication_failure:no_password_set", values.User.Id);
    }

    [Fact]
    public async Task Authenticate_UserHasNoPassword_ReturnsNull()
    {
        var values = this.SetupAuthenticate_UserHasNoPassword();

        Assert.Null(
            await this.passwordAuthenticationService.Authenticate(
                values.SuppliedEmail, values.SuppliedPassword));
    }

    [Fact]
    public async Task Authenticate_IncorrectPassword_LogsIncorrectPassword()
    {
        var values = this.SetupAuthenticate_IncorrectPassword();

        await this.passwordAuthenticationService.Authenticate(
            values.SuppliedEmail, values.SuppliedPassword);

        LogAssert.HasEntry(
            this.logger,
            LogLevel.Information,
            200,
            $"Authentication failed; incorrect password for user {values.User.Id} ({values.User.Email})");
    }

    [Fact]
    public async Task Authenticate_IncorrectPassword_InsertsSecurityEvent()
    {
        var values = this.SetupAuthenticate_IncorrectPassword();

        await this.passwordAuthenticationService.Authenticate(
            values.SuppliedEmail, values.SuppliedPassword);

        this.AssertSecurityEventLogged("authentication_failure:incorrect_password", values.User.Id);
    }

    [Fact]
    public async Task Authenticate_IncorrectPassword_ReturnsNull()
    {
        var values = this.SetupAuthenticate_IncorrectPassword();

        Assert.Null(
            await this.passwordAuthenticationService.Authenticate(
                values.SuppliedEmail, values.SuppliedPassword));
    }

    private sealed record AuthenticateValues(
        string SuppliedEmail, string SuppliedPassword, User User);

    private AuthenticateValues BuildAuthenticateValues(string? hashedPassword) =>
        new(
            this.modelFactory.NextEmail(),
            this.modelFactory.NextString("password"),
            this.modelFactory.BuildUser() with { HashedPassword = hashedPassword });

    private AuthenticateValues SetupAuthenticate_EmailRecognized(
        string? hashedPassword)
    {
        var values = this.BuildAuthenticateValues(hashedPassword);
        this.SetupFindUserByEmail(values.SuppliedEmail, values.User);
        return values;
    }

    private AuthenticateValues SetupAuthenticate_EmailUnrecognized()
    {
        var values = this.BuildAuthenticateValues(null);
        this.SetupFindUserByEmail(values.SuppliedEmail, null);
        return values;
    }

    private AuthenticateValues SetupAuthenticate_IncorrectPassword() =>
        this.SetupAuthenticate_UserHasPassword(PasswordVerificationResult.Failed);

    private AuthenticateValues SetupAuthenticate_Success() =>
        this.SetupAuthenticate_UserHasPassword(PasswordVerificationResult.Success);

    private AuthenticateValues SetupAuthenticate_UserHasNoPassword() =>
        this.SetupAuthenticate_EmailRecognized(null);

    private AuthenticateValues SetupAuthenticate_UserHasPassword(
        PasswordVerificationResult passwordVerificationResult)
    {
        var hashedPassword = this.modelFactory.NextString("hashed-password");
        var values = this.SetupAuthenticate_EmailRecognized(hashedPassword);

        this.passwordHasherMock
            .Setup(
                x => x.VerifyHashedPassword(values.User, hashedPassword, values.SuppliedPassword))
            .Returns(passwordVerificationResult);

        return values;
    }

    #endregion

    #region ChangePassword

    [Fact]
    public async Task ChangePassword_UserHasNoPassword_InsertsSecurityEvent()
    {
        var values = this.SetupChangePassword_UserHasNoPassword();

        try
        {
            await this.passwordAuthenticationService.ChangePassword(
                values.User.Id, values.SuppliedCurrentPassword, values.NewPassword);
        }
        catch (InvalidOperationException)
        {
        }

        this.AssertSecurityEventLogged("password_change_failure:no_password_set", values.User.Id);
    }

    [Fact]
    public async Task ChangePassword_UserHasNoPassword_ThrowsException()
    {
        var values = this.SetupChangePassword_UserHasNoPassword();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => this.passwordAuthenticationService.ChangePassword(
                values.User.Id, values.SuppliedCurrentPassword, values.NewPassword));
    }

    [Fact]
    public async Task ChangePassword_IncorrectCurrentPassword_LogsIncorrectPassword()
    {
        var values = this.SetupChangePassword_IncorrectCurrentPassword();

        await this.passwordAuthenticationService.ChangePassword(
            values.User.Id, values.SuppliedCurrentPassword, values.NewPassword);

        LogAssert.HasEntry(
            this.logger,
            LogLevel.Information,
            204,
            $"Password change denied for user {values.User.Id} ({values.User.Email}); current password is incorrect");
    }

    [Fact]
    public async Task ChangePassword_IncorrectCurrentPassword_InsertsSecurityEvent()
    {
        var values = this.SetupChangePassword_IncorrectCurrentPassword();

        await this.passwordAuthenticationService.ChangePassword(
            values.User.Id, values.SuppliedCurrentPassword, values.NewPassword);

        this.AssertSecurityEventLogged(
            "password_change_failure:incorrect_password", values.User.Id);
    }

    [Fact]
    public async Task ChangePassword_IncorrectCurrentPassword_ReturnsFalse()
    {
        var values = this.SetupChangePassword_IncorrectCurrentPassword();

        Assert.False(
            await this.passwordAuthenticationService.ChangePassword(
                values.User.Id, values.SuppliedCurrentPassword, values.NewPassword));
    }

    [Fact]
    public async Task ChangePassword_IncorrectCurrentPassword_DoesNotChangePassword()
    {
        var values = this.SetupChangePassword_IncorrectCurrentPassword();

        await this.passwordAuthenticationService.ChangePassword(
            values.User.Id, values.SuppliedCurrentPassword, values.NewPassword);

        this.passwordHasherMock.Verify(
            x => x.HashPassword(values.User, values.NewPassword), Times.Never);
    }

    [Fact]
    public async Task ChangePassword_Success_UpdatesUser()
    {
        var values = this.SetupChangePassword_Success();

        await this.passwordAuthenticationService.ChangePassword(
            values.User.Id, values.SuppliedCurrentPassword, values.NewPassword);

        this.userDataProviderMock.Verify(x => x.UpdatePassword(
            this.dbContextFactory.FakeDbContext,
            values.User.Id,
            values.NewHashedPassword,
            values.NewSecurityStamp));
    }

    [Fact]
    public async Task ChangePassword_Success_DeletesPasswordResetTokens()
    {
        var values = this.SetupChangePassword_Success();

        await this.passwordAuthenticationService.ChangePassword(
            values.User.Id, values.SuppliedCurrentPassword, values.NewPassword);

        this.passwordResetTokenDataProviderMock.Verify(
            x => x.DeleteTokensForUser(this.dbContextFactory.FakeDbContext, values.User.Id));
    }

    [Fact]
    public async Task ChangePassword_Success_SendsPasswordChangeNotification()
    {
        var values = this.SetupChangePassword_Success();

        await this.passwordAuthenticationService.ChangePassword(
            values.User.Id, values.SuppliedCurrentPassword, values.NewPassword);

        this.authenticationMailerMock.Verify(
            x => x.SendPasswordChangeNotification(values.User.Email));
    }

    [Fact]
    public async Task ChangePassword_Success_LogsPasswordChanged()
    {
        var values = this.SetupChangePassword_Success();

        await this.passwordAuthenticationService.ChangePassword(
            values.User.Id, values.SuppliedCurrentPassword, values.NewPassword);

        LogAssert.HasEntry(
            this.logger,
            LogLevel.Information,
            205,
            $"Password successfully changed for user {values.User.Id} ({values.User.Email})");
    }

    [Fact]
    public async Task ChangePassword_Success_InsertsSecurityEvent()
    {
        var values = this.SetupChangePassword_Success();

        await this.passwordAuthenticationService.ChangePassword(
            values.User.Id, values.SuppliedCurrentPassword, values.NewPassword);

        this.AssertSecurityEventLogged("password_change_success", values.User.Id);
    }

    [Fact]
    public async Task ChangePassword_Success_ReturnsTrue()
    {
        var values = this.SetupChangePassword_Success();

        Assert.True(
            await this.passwordAuthenticationService.ChangePassword(
                values.User.Id, values.SuppliedCurrentPassword, values.NewPassword));
    }

    private sealed record ChangePasswordValues(
        string NewHashedPassword,
        string NewPassword,
        string NewSecurityStamp,
        string SuppliedCurrentPassword,
        User User);

    private ChangePasswordValues SetupChangePassword(string? currentHashedPassword)
    {
        var values = new ChangePasswordValues(
            this.modelFactory.NextString("new-hashed-password"),
            this.modelFactory.NextString("new-password"),
            this.modelFactory.NextString("new-security-stamp"),
            this.modelFactory.NextString("supplied-current-password"),
            this.modelFactory.BuildUser() with { HashedPassword = currentHashedPassword });

        this.SetupGetUser(values.User.Id, values.User);

        this.passwordHasherMock
            .Setup(x => x.HashPassword(values.User, values.NewPassword))
            .Returns(values.NewHashedPassword);

        this.randomTokenGeneratorMock
            .Setup(x => x.Generate(2))
            .Returns(values.NewSecurityStamp);

        return values;
    }

    private ChangePasswordValues SetupChangePassword_IncorrectCurrentPassword() =>
        this.SetupChangePassword_UserHasPassword(PasswordVerificationResult.Failed);

    private ChangePasswordValues SetupChangePassword_Success() =>
        this.SetupChangePassword_UserHasPassword(PasswordVerificationResult.Success);

    private ChangePasswordValues SetupChangePassword_UserHasPassword(
        PasswordVerificationResult passwordVerificationResult)
    {
        var currentHashedPassword = this.modelFactory.NextString("current-hashed-password");
        var values = this.SetupChangePassword(currentHashedPassword);

        this.passwordHasherMock
            .Setup(x => x.VerifyHashedPassword(
                values.User, currentHashedPassword, values.SuppliedCurrentPassword))
            .Returns(passwordVerificationResult);

        return values;
    }

    private ChangePasswordValues SetupChangePassword_UserHasNoPassword() =>
        this.SetupChangePassword(null);

    #endregion

    #region PasswordResetTokenIsValid

    [Fact]
    public async Task PasswordResetTokenIsValid_DeletesExpiredTokens()
    {
        var values = this.SetupPasswordResetTokenIsValid(true);

        await this.passwordAuthenticationService.PasswordResetTokenIsValid(values.Token);

        this.passwordResetTokenDataProviderMock.Verify(
            x => x.DeleteExpiredTokens(
                this.dbContextFactory.FakeDbContext, this.clock.UtcNow.AddDays(-1)));
    }

    [Fact]
    public async Task PasswordResetTokenIsValid_Valid_LogsValidToken()
    {
        var values = this.SetupPasswordResetTokenIsValid(true);

        await this.passwordAuthenticationService.PasswordResetTokenIsValid(values.Token);

        LogAssert.HasEntry(
            this.logger,
            LogLevel.Debug,
            207,
            $"Password reset token '{values.Token[..6]}…' is valid and belongs to user {values.UserId}");
    }

    [Fact]
    public async Task PasswordResetTokenIsValid_Valid_ReturnsTrue()
    {
        var values = this.SetupPasswordResetTokenIsValid(true);

        Assert.True(
            await this.passwordAuthenticationService.PasswordResetTokenIsValid(values.Token));
    }

    [Fact]
    public async Task PasswordResetTokenIsValid_Invalid_LogsInvalidToken()
    {
        var values = this.SetupPasswordResetTokenIsValid(false);

        await this.passwordAuthenticationService.PasswordResetTokenIsValid(values.Token);

        LogAssert.HasEntry(
            this.logger,
            LogLevel.Debug,
            206,
            $"Password reset token '{values.Token[..6]}…' is no longer valid");
    }

    [Fact]
    public async Task PasswordResetTokenIsValid_Invalid_InsertsSecurityEvent()
    {
        var values = this.SetupPasswordResetTokenIsValid(false);

        await this.passwordAuthenticationService.PasswordResetTokenIsValid(values.Token);

        this.AssertSecurityEventLogged("password_reset_failure:invalid_token");
    }

    [Fact]
    public async Task PasswordResetTokenIsValid_Invalid_ReturnsFalse()
    {
        var values = this.SetupPasswordResetTokenIsValid(false);

        Assert.False(
            await this.passwordAuthenticationService.PasswordResetTokenIsValid(values.Token));
    }

    private sealed record PasswordResetTokenIsValidValues(string Token, long UserId);

    private PasswordResetTokenIsValidValues SetupPasswordResetTokenIsValid(bool valid)
    {
        var token = this.modelFactory.NextString("token");
        var userId = this.modelFactory.NextInt();

        this.SetupGetUserIdForToken(token, valid ? userId : null);

        return new(token, userId);
    }

    #endregion

    #region ResetPassword

    [Fact]
    public async Task ResetPassword_DeletesExpiredPasswordResetTokens()
    {
        var values = this.SetupResetPassword(true);

        await this.passwordAuthenticationService.ResetPassword(values.Token, values.NewPassword);

        this.passwordResetTokenDataProviderMock.Verify(
            x => x.DeleteExpiredTokens(
                this.dbContextFactory.FakeDbContext, this.clock.UtcNow.AddDays(-1)));
    }

    [Fact]
    public async Task ResetPassword_InvalidToken_LogsInvalidToken()
    {
        var values = this.SetupResetPassword(false);

        try
        {
            await this.passwordAuthenticationService.ResetPassword(
                values.Token, values.NewPassword);
        }
        catch (InvalidTokenException)
        {
        }

        LogAssert.HasEntry(
            this.logger,
            LogLevel.Information,
            208,
            $"Unable to reset password; password reset token {values.Token[..6]}… is invalid");
    }

    [Fact]
    public async Task ResetPassword_InvalidToken_InsertsSecurityEvent()
    {
        var values = this.SetupResetPassword(false);

        try
        {
            await this.passwordAuthenticationService.ResetPassword(
                values.Token, values.NewPassword);
        }
        catch (InvalidTokenException)
        {
        }

        this.AssertSecurityEventLogged("password_reset_failure:invalid_token");
    }

    [Fact]
    public async Task ResetPassword_InvalidToken_Throws()
    {
        var values = this.SetupResetPassword(false);

        await Assert.ThrowsAsync<InvalidTokenException>(
            () => this.passwordAuthenticationService.ResetPassword(
                values.Token, values.NewPassword));
    }

    [Fact]
    public async Task ResetPassword_Success_UpdatesUser()
    {
        var values = this.SetupResetPassword(true);

        await this.passwordAuthenticationService.ResetPassword(values.Token, values.NewPassword);

        this.userDataProviderMock.Verify(
            x => x.UpdatePassword(
                this.dbContextFactory.FakeDbContext,
                values.User.Id,
                values.NewHashedPassword,
                values.NewSecurityStamp));
    }

    [Fact]
    public async Task ResetPassword_Success_DeletesPasswordResetTokens()
    {
        var values = this.SetupResetPassword(true);

        await this.passwordAuthenticationService.ResetPassword(values.Token, values.NewPassword);

        this.passwordResetTokenDataProviderMock.Verify(
            x => x.DeleteTokensForUser(this.dbContextFactory.FakeDbContext, values.User.Id));
    }

    [Fact]
    public async Task ResetPassword_Success_SendsPasswordChangeNotification()
    {
        var values = this.SetupResetPassword(true);

        await this.passwordAuthenticationService.ResetPassword(values.Token, values.NewPassword);

        this.authenticationMailerMock.Verify(
            x => x.SendPasswordChangeNotification(values.User.Email));
    }

    [Fact]
    public async Task ResetPassword_Success_LogsPasswordReset()
    {
        var values = this.SetupResetPassword(true);

        await this.passwordAuthenticationService.ResetPassword(values.Token, values.NewPassword);

        LogAssert.HasEntry(
            this.logger,
            LogLevel.Information,
            209,
            $"Password reset for user {values.User.Id} using token {values.Token[..6]}…");
    }

    [Fact]
    public async Task ResetPassword_Success_InsertsSecurityEvent()
    {
        var values = this.SetupResetPassword(true);

        await this.passwordAuthenticationService.ResetPassword(values.Token, values.NewPassword);

        this.AssertSecurityEventLogged("password_reset_success", values.User.Id);
    }

    [Fact]
    public async Task ResetPassword_Success_ReturnsUserWithNewSecurityStamp()
    {
        var values = this.SetupResetPassword(true);

        var returnedUser = await this.passwordAuthenticationService.ResetPassword(
            values.Token, values.NewPassword);

        Assert.Equal(values.User with { SecurityStamp = values.NewSecurityStamp }, returnedUser);
    }

    private sealed record ResetPasswordValues(
        string NewHashedPassword,
        string NewPassword,
        string NewSecurityStamp,
        string Token,
        User User);

    private ResetPasswordValues SetupResetPassword(bool tokenIsValid)
    {
        var newHashedPassword = this.modelFactory.NextString("new-hashed-password");
        var newPassword = this.modelFactory.NextString("new-password");
        var newSecurityStamp = this.modelFactory.NextString("new-security-stamp");
        var token = this.modelFactory.NextString("token");
        var user = this.modelFactory.BuildUser();

        this.SetupGetUserIdForToken(token, tokenIsValid ? user.Id : null);
        this.SetupGetUser(user.Id, user);
        this.passwordHasherMock
            .Setup(x => x.HashPassword(user, newPassword))
            .Returns(newHashedPassword);
        this.randomTokenGeneratorMock
            .Setup(x => x.Generate(2))
            .Returns(newSecurityStamp);

        return new(newHashedPassword, newPassword, newSecurityStamp, token, user);
    }

    #endregion

    #region SendPasswordResetLink

    [Fact]
    public async Task SendPasswordResetLink_Success_InsertsPasswordResetToken()
    {
        var values = this.SetupSendPasswordResetLink(true);

        await this.passwordAuthenticationService.SendPasswordResetLink(
            values.Email, values.UrlHelper);

        this.passwordResetTokenDataProviderMock.Verify(
            x => x.InsertToken(this.dbContextFactory.FakeDbContext, values.User.Id, values.Token));
    }

    [Fact]
    public async Task SendPasswordResetLink_Success_SendsLinkToUser()
    {
        var values = this.SetupSendPasswordResetLink(true);

        await this.passwordAuthenticationService.SendPasswordResetLink(
            values.Email, values.UrlHelper);

        this.authenticationMailerMock.Verify(
            x => x.SendPasswordResetLink(values.User.Email, values.Link));
    }

    [Fact]
    public async Task SendPasswordResetLink_Success_LogsLinkSent()
    {
        var values = this.SetupSendPasswordResetLink(true);

        await this.passwordAuthenticationService.SendPasswordResetLink(
            values.Email, values.UrlHelper);

        LogAssert.HasEntry(
            this.logger,
            LogLevel.Information,
            210,
            $"Password reset link sent to user {values.User.Id} ({values.User.Email})");
    }

    [Fact]
    public async Task SendPasswordResetLink_Success_InsertsSecurityEvent()
    {
        var values = this.SetupSendPasswordResetLink(true);

        await this.passwordAuthenticationService.SendPasswordResetLink(
            values.Email, values.UrlHelper);

        this.AssertSecurityEventLogged("password_reset_link_sent", values.User.Id);
    }

    [Fact]
    public async Task SendPasswordResetLink_EmailIsUnrecognized_DoesNotSendLink()
    {
        var values = this.SetupSendPasswordResetLink(false);

        await this.passwordAuthenticationService.SendPasswordResetLink(
            values.Email, values.UrlHelper);

        this.authenticationMailerMock.Verify(
            x => x.SendPasswordResetLink(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SendPasswordResetLink_EmailIsUnrecognized_LogsUnrecognizedEmail()
    {
        var values = this.SetupSendPasswordResetLink(false);

        await this.passwordAuthenticationService.SendPasswordResetLink(
            values.Email, values.UrlHelper);

        LogAssert.HasEntry(
            this.logger,
            LogLevel.Information,
            211,
            $"Unable to send password reset link; No user with email {values.Email}");
    }

    [Fact]
    public async Task SendPasswordResetLink_EmailIsUnrecognized_InsertsSecurityEvent()
    {
        var values = this.SetupSendPasswordResetLink(false);

        await this.passwordAuthenticationService.SendPasswordResetLink(
            values.Email, values.UrlHelper);

        this.AssertSecurityEventLogged("password_reset_failure:unrecognized_email");
    }

    private sealed record SendPasswordResetLinkValues(
        string Email, string Link, string Token, IUrlHelper UrlHelper, User User);

    private SendPasswordResetLinkValues SetupSendPasswordResetLink(bool emailIsRecognized)
    {
        var email = this.modelFactory.NextEmail();
        var link = this.modelFactory.NextString("https://example.com/reset-password/token");
        var urlHelperMock = new Mock<IUrlHelper>();
        var user = this.modelFactory.BuildUser();
        var token = this.modelFactory.NextString("token");

        this.userDataProviderMock
            .Setup(x => x.FindUserByEmail(this.dbContextFactory.FakeDbContext, email))
            .ReturnsAsync(emailIsRecognized ? user : null);
        this.randomTokenGeneratorMock
            .Setup(x => x.Generate(12))
            .Returns(token);
        urlHelperMock
            .Setup(
                x => x.Link(
                    "ResetPassword",
                    It.Is<object>(o => token.Equals(new RouteValueDictionary(o)["token"]))))
            .Returns(link);

        return new(email, link, token, urlHelperMock.Object, user);
    }

    #endregion

    private void AssertSecurityEventLogged(string eventName, long? userId = null) =>
        this.securityEventDataProviderMock.Verify(x => x.LogEvent(
            this.dbContextFactory.FakeDbContext, eventName, userId));

    private void SetupFindUserByEmail(string email, User? user) =>
        this.userDataProviderMock
            .Setup(x => x.FindUserByEmail(this.dbContextFactory.FakeDbContext, email))
            .ReturnsAsync(user);

    private void SetupGetUser(long id, User user) =>
        this.userDataProviderMock
            .Setup(x => x.GetUser(this.dbContextFactory.FakeDbContext, id))
            .ReturnsAsync(user);

    private void SetupGetUserIdForToken(string token, long? userId) =>
        this.passwordResetTokenDataProviderMock
            .Setup(x => x.GetUserIdForToken(this.dbContextFactory.FakeDbContext, token))
            .ReturnsAsync(userId);
}
