using System.Net;
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
    public async Task Authenticate_EmailUnrecognized()
    {
        var args = this.BuildAuthenticateArgs();

        this.SetupFindUserByEmail(args.Email, null);

        var result = await this.passwordAuthenticationService.Authenticate(
            args.Email, args.Password, args.IpAddress);

        // Logs unrecognized email message
        LogAssert.HasEntry(
            this.logger,
            LogLevel.Information,
            203,
            $"Authentication failed; no user with email {args.Email}");

        // Inserts security event
        this.AssertSecurityEventLogged("authentication_failure:unrecognized_email", args.IpAddress);

        // Returns null
        Assert.Null(result);
    }

    [Fact]
    public async Task Authenticate_UserHasNoPassword()
    {
        var args = this.BuildAuthenticateArgs();
        var user = this.modelFactory.BuildUser() with { HashedPassword = null };

        this.SetupFindUserByEmail(args.Email, user);

        var result = await this.passwordAuthenticationService.Authenticate(
            args.Email, args.Password, args.IpAddress);

        // Logs no password set message
        LogAssert.HasEntry(
            this.logger,
            LogLevel.Information,
            201,
            $"Authentication failed; no password set for user {user.Id} ({user.Email})");

        // Inserts security event
        this.AssertSecurityEventLogged(
            "authentication_failure:no_password_set", args.IpAddress, user.Id);

        // Returns null
        Assert.Null(result);
    }

    [Fact]
    public async Task Authenticate_IncorrectPassword()
    {
        var args = this.BuildAuthenticateArgs();
        var hashedPassword = this.modelFactory.NextString("hashed-password");
        var user = this.modelFactory.BuildUser() with { HashedPassword = hashedPassword };

        this.SetupFindUserByEmail(args.Email, user);
        this.SetupVerifyHashedPassword(
            user, hashedPassword, args.Password, PasswordVerificationResult.Failed);

        var result = await this.passwordAuthenticationService.Authenticate(
            args.Email, args.Password, args.IpAddress);

        // Logs incorrect password message
        LogAssert.HasEntry(
            this.logger,
            LogLevel.Information,
            200,
            $"Authentication failed; incorrect password for user {user.Id} ({user.Email})");

        // Inserts security event
        this.AssertSecurityEventLogged(
            "authentication_failure:incorrect_password", args.IpAddress, user.Id);

        // Returns null
        Assert.Null(result);
    }

    [Fact]
    public async Task Authenticate_Success()
    {
        var args = this.BuildAuthenticateArgs();
        var hashedPassword = this.modelFactory.NextString("hashed-password");
        var user = this.modelFactory.BuildUser() with { HashedPassword = hashedPassword };

        this.SetupFindUserByEmail(args.Email, user);
        this.SetupVerifyHashedPassword(
            user, hashedPassword, args.Password, PasswordVerificationResult.Success);

        var result = await this.passwordAuthenticationService.Authenticate(
            args.Email, args.Password, args.IpAddress);

        // Logs successfully authenticated message
        LogAssert.HasEntry(
            this.logger,
            LogLevel.Information,
            202,
            $"User {user.Id} ({user.Email}) successfully authenticated");

        // Inserts security event
        this.AssertSecurityEventLogged("authentication_success", args.IpAddress, user.Id);

        // Returns user
        Assert.Equal(user, result);
    }

    private sealed record AuthenticateArgs(string Email, string Password, IPAddress IpAddress);

    private AuthenticateArgs BuildAuthenticateArgs() => new(
        this.modelFactory.NextEmail(),
        this.modelFactory.NextString("password"),
        new(this.modelFactory.NextInt()));

    #endregion

    #region ChangePassword

    [Fact]
    public async Task ChangePassword_UserHasNoPassword()
    {
        var args = this.BuildChangePasswordArgs();
        var user = this.modelFactory.BuildUser() with { HashedPassword = null };

        this.SetupGetUser(args.UserId, user);

        // Throws exception
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => this.passwordAuthenticationService.ChangePassword(
                args.UserId, args.CurrentPassword, args.NewPassword, args.IpAddress));

        // Inserts security event
        this.AssertSecurityEventLogged(
            "password_change_failure:no_password_set", args.IpAddress, user.Id);
    }

    [Fact]
    public async Task ChangePassword_IncorrectCurrentPassword()
    {
        var args = this.BuildChangePasswordArgs();
        var currentPasswordHash = this.modelFactory.NextString("hashed-current-password");
        var user = this.modelFactory.BuildUser() with { HashedPassword = currentPasswordHash };

        this.SetupGetUser(args.UserId, user);
        this.SetupVerifyHashedPassword(
            user, currentPasswordHash, args.CurrentPassword, PasswordVerificationResult.Failed);

        var result = await this.passwordAuthenticationService.ChangePassword(
            args.UserId, args.CurrentPassword, args.NewPassword, args.IpAddress);

        // Does not attempt to hash new password
        this.passwordHasherMock.Verify(x => x.HashPassword(user, args.NewPassword), Times.Never);

        // Logs password incorrect message
        LogAssert.HasEntry(
            this.logger,
            LogLevel.Information,
            204,
            $"Password change denied for user {user.Id} ({user.Email}); current password is incorrect");

        // Inserts security event
        this.AssertSecurityEventLogged(
            "password_change_failure:incorrect_password", args.IpAddress, user.Id);

        // Returns false
        Assert.False(result);
    }

    [Fact]
    public async Task ChangePassword_Success()
    {
        var args = this.BuildChangePasswordArgs();
        var currentPasswordHash = this.modelFactory.NextString("current-password-hash");
        var user = this.modelFactory.BuildUser() with { HashedPassword = currentPasswordHash };

        this.SetupGetUser(args.UserId, user);
        this.SetupVerifyHashedPassword(
            user, currentPasswordHash, args.CurrentPassword, PasswordVerificationResult.Success);
        var newPasswordHash = this.SetupHashPassword(user, args.NewPassword);
        var newSecurityStamp = this.SetupGenerateSecurityStamp();

        var result = await this.passwordAuthenticationService.ChangePassword(
            args.UserId, args.CurrentPassword, args.NewPassword, args.IpAddress);

        // Updates user's password hash and security stamp
        this.userDataProviderMock.Verify(x => x.UpdatePassword(
            this.dbContextFactory.FakeDbContext, user.Id, newPasswordHash, newSecurityStamp));

        // Deletes all previously issued password reset tokens for the user
        this.passwordResetTokenDataProviderMock.Verify(
            x => x.DeleteTokensForUser(this.dbContextFactory.FakeDbContext, user.Id));

        // Sends password change notification
        this.authenticationMailerMock.Verify(x => x.SendPasswordChangeNotification(user.Email));

        // Logs password changed message
        LogAssert.HasEntry(
            this.logger,
            LogLevel.Information,
            205,
            $"Password successfully changed for user {user.Id} ({user.Email})");

        // Inserts security event
        this.AssertSecurityEventLogged("password_change_success", args.IpAddress, user.Id);

        // Returns true
        Assert.True(result);
    }

    private sealed record ChangePasswordArgs(
        long UserId, string CurrentPassword, string NewPassword, IPAddress IpAddress);

    private ChangePasswordArgs BuildChangePasswordArgs() => new(
        this.modelFactory.NextInt(),
        this.modelFactory.NextString("current-password"),
        this.modelFactory.NextString("new-password"),
        new(this.modelFactory.NextInt()));

    #endregion

    #region PasswordResetTokenIsValid

    [Fact]
    public async Task PasswordResetTokenIsValid_Valid()
    {
        var args = this.BuildPasswordResetTokenIsValidArgs();
        var userId = this.modelFactory.NextInt();

        this.SetupGetUserIdForToken(args.Token, userId);

        var result = await this.passwordAuthenticationService.PasswordResetTokenIsValid(
            args.Token, args.IpAddress);

        // Deletes expired password reset tokens
        this.passwordResetTokenDataProviderMock.Verify(
            x => x.DeleteExpiredTokens(
                this.dbContextFactory.FakeDbContext, this.clock.UtcNow.AddDays(-1)));

        // Logs valid token message
        LogAssert.HasEntry(
            this.logger,
            LogLevel.Debug,
            207,
            $"Password reset token '{args.Token[..6]}…' is valid and belongs to user {userId}");

        // Returns true
        Assert.True(result);
    }

    [Fact]
    public async Task PasswordResetTokenIsValid_Invalid()
    {
        var args = this.BuildPasswordResetTokenIsValidArgs();

        this.SetupGetUserIdForToken(args.Token, null);

        var result = await this.passwordAuthenticationService.PasswordResetTokenIsValid(
            args.Token, args.IpAddress);

        // Deletes expired password reset tokens
        this.passwordResetTokenDataProviderMock.Verify(
            x => x.DeleteExpiredTokens(
                this.dbContextFactory.FakeDbContext, this.clock.UtcNow.AddDays(-1)));

        // Logs invalid token message
        LogAssert.HasEntry(
            this.logger,
            LogLevel.Debug,
            206,
            $"Password reset token '{args.Token[..6]}…' is no longer valid");

        // Inserts security event
        this.AssertSecurityEventLogged("password_reset_failure:invalid_token", args.IpAddress);

        // Returns false
        Assert.False(result);
    }

    private sealed record PasswordResetTokenIsValidArgs(string Token, IPAddress IpAddress);

    private PasswordResetTokenIsValidArgs BuildPasswordResetTokenIsValidArgs() => new(
        this.modelFactory.NextString("token"), new(this.modelFactory.NextInt()));

    #endregion

    #region ResetPassword

    [Fact]
    public async Task ResetPassword_InvalidToken()
    {
        var args = this.BuildResetPasswordArgs();

        this.SetupGetUserIdForToken(args.Token, null);

        // Throws exception
        await Assert.ThrowsAsync<InvalidTokenException>(
            () => this.passwordAuthenticationService.ResetPassword(
                args.Token, args.NewPassword, args.IpAddress));

        // Deletes expired password reset tokens
        this.passwordResetTokenDataProviderMock.Verify(
            x => x.DeleteExpiredTokens(
                this.dbContextFactory.FakeDbContext, this.clock.UtcNow.AddDays(-1)));

        // Logs invalid token message
        LogAssert.HasEntry(
            this.logger,
            LogLevel.Information,
            208,
            $"Unable to reset password; password reset token {args.Token[..6]}… is invalid");

        // Inserts security event
        this.AssertSecurityEventLogged("password_reset_failure:invalid_token", args.IpAddress);
    }

    [Fact]
    public async Task ResetPassword_Success()
    {
        var args = this.BuildResetPasswordArgs();
        var user = this.modelFactory.BuildUser();

        this.SetupGetUserIdForToken(args.Token, user.Id);
        this.SetupGetUser(user.Id, user);
        var newPasswordHash = this.SetupHashPassword(user, args.NewPassword);
        var newSecurityStamp = this.SetupGenerateSecurityStamp();

        var result = await this.passwordAuthenticationService.ResetPassword(
            args.Token, args.NewPassword, args.IpAddress);

        // Deletes expired password reset tokens
        this.passwordResetTokenDataProviderMock.Verify(
            x => x.DeleteExpiredTokens(
                this.dbContextFactory.FakeDbContext, this.clock.UtcNow.AddDays(-1)));

        // Updates user's password hash and security stamp
        this.userDataProviderMock.Verify(
            x => x.UpdatePassword(
                this.dbContextFactory.FakeDbContext, user.Id, newPasswordHash, newSecurityStamp));

        // Deletes all previously issued password reset tokens for the user
        this.passwordResetTokenDataProviderMock.Verify(
            x => x.DeleteTokensForUser(this.dbContextFactory.FakeDbContext, user.Id));

        // Logs password reset message
        LogAssert.HasEntry(
            this.logger,
            LogLevel.Information,
            209,
            $"Password reset for user {user.Id} using token {args.Token[..6]}…");

        // Inserts security event
        this.AssertSecurityEventLogged("password_reset_success", args.IpAddress, user.Id);

        // Sends password change notification
        this.authenticationMailerMock.Verify(x => x.SendPasswordChangeNotification(user.Email));

        // Returns user with updated security stamp
        Assert.Equal(user with { SecurityStamp = newSecurityStamp }, result);
    }

    private sealed record ResetPasswordArgs(string Token, string NewPassword, IPAddress IpAddress);

    private ResetPasswordArgs BuildResetPasswordArgs() => new(
        this.modelFactory.NextString("token"),
        this.modelFactory.NextString("new-password"),
        new(this.modelFactory.NextInt()));

    #endregion

    #region SendPasswordResetLink

    [Fact]
    public async Task SendPasswordResetLink_EmailUnrecognized()
    {
        var args = this.BuildSendPasswordResetLinkArgs();

        this.SetupFindUserByEmail(args.Email, null);

        await this.passwordAuthenticationService.SendPasswordResetLink(
            args.Email, args.IpAddress, Mock.Of<IUrlHelper>());

        // Logs unrecognised email message
        LogAssert.HasEntry(
            this.logger,
            LogLevel.Information,
            211,
            $"Unable to send password reset link; No user with email {args.Email}");

        // Inserts security event
        this.AssertSecurityEventLogged("password_reset_failure:unrecognized_email", args.IpAddress);
    }

    [Fact]
    public async Task SendPasswordResetLink_Success()
    {
        var args = this.BuildSendPasswordResetLinkArgs();
        var user = this.modelFactory.BuildUser();
        var token = this.modelFactory.NextString("token");
        var link = this.modelFactory.NextString("https://example.com/reset-password/token");

        this.SetupFindUserByEmail(args.Email, user);
        this.randomTokenGeneratorMock.Setup(x => x.Generate(12)).Returns(token);

        var urlHelperMock = new Mock<IUrlHelper>();
        urlHelperMock
            .Setup(
                x => x.Link(
                    "ResetPassword",
                    It.Is<object>(o => token.Equals(new RouteValueDictionary(o)["token"]))))
            .Returns(link);

        await this.passwordAuthenticationService.SendPasswordResetLink(
            args.Email, args.IpAddress, urlHelperMock.Object);

        // Insert password reset token
        this.passwordResetTokenDataProviderMock.Verify(
            x => x.InsertToken(this.dbContextFactory.FakeDbContext, user.Id, token));

        // Sends link to user
        this.authenticationMailerMock.Verify(x => x.SendPasswordResetLink(user.Email, link));

        // Logs password reset link sent message
        LogAssert.HasEntry(
            this.logger,
            LogLevel.Information,
            210,
            $"Password reset link sent to user {user.Id} ({user.Email})");

        // Inserts security event
        this.AssertSecurityEventLogged("password_reset_link_sent", args.IpAddress, user.Id);
    }

    private sealed record SendPasswordResetLinkArgs(string Email, IPAddress IpAddress);

    private SendPasswordResetLinkArgs BuildSendPasswordResetLinkArgs() =>
        new(this.modelFactory.NextEmail(), new(this.modelFactory.NextInt()));

    #endregion

    private void AssertSecurityEventLogged(
        string eventName, IPAddress ipAddress, long? userId = null) =>
        this.securityEventDataProviderMock.Verify(x => x.LogEvent(
            this.dbContextFactory.FakeDbContext, eventName, ipAddress, userId));

    private void SetupFindUserByEmail(string email, User? user) =>
        this.userDataProviderMock
            .Setup(x => x.FindUserByEmail(this.dbContextFactory.FakeDbContext, email))
            .ReturnsAsync(user);

    private string SetupGenerateSecurityStamp()
    {
        var securityStamp = this.modelFactory.NextString("security-stamp");
        this.randomTokenGeneratorMock.Setup(x => x.Generate(2)).Returns(securityStamp);
        return securityStamp;
    }

    private void SetupGetUser(long id, User user) =>
        this.userDataProviderMock
            .Setup(x => x.GetUser(this.dbContextFactory.FakeDbContext, id))
            .ReturnsAsync(user);

    private void SetupGetUserIdForToken(string token, long? userId) =>
        this.passwordResetTokenDataProviderMock
            .Setup(x => x.GetUserIdForToken(this.dbContextFactory.FakeDbContext, token))
            .ReturnsAsync(userId);

    private string SetupHashPassword(User user, string newPassword)
    {
        var passwordHash = this.modelFactory.NextString("password-hash");
        this.passwordHasherMock.Setup(x => x.HashPassword(user, newPassword)).Returns(passwordHash);
        return passwordHash;
    }

    private void SetupVerifyHashedPassword(
        User user,
        string hashedPassword,
        string suppliedPassword,
        PasswordVerificationResult result) =>
        this.passwordHasherMock
            .Setup(x => x.VerifyHashedPassword(user, hashedPassword, suppliedPassword))
            .Returns(result);
}
