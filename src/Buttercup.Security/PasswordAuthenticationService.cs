using System.Net;
using Buttercup.DataAccess;
using Buttercup.EntityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Buttercup.Security;

internal sealed class PasswordAuthenticationService : IPasswordAuthenticationService
{
    private readonly IAuthenticationMailer authenticationMailer;
    private readonly IClock clock;
    private readonly IDbContextFactory<AppDbContext> dbContextFactory;
    private readonly ILogger<PasswordAuthenticationService> logger;
    private readonly IPasswordHasher<User> passwordHasher;
    private readonly IPasswordResetTokenDataProvider passwordResetTokenDataProvider;
    private readonly IRandomTokenGenerator randomTokenGenerator;
    private readonly ISecurityEventDataProvider securityEventDataProvider;
    private readonly IUserDataProvider userDataProvider;

    public PasswordAuthenticationService(
        IAuthenticationMailer authenticationMailer,
        IClock clock,
        IDbContextFactory<AppDbContext> dbContextFactory,
        ILogger<PasswordAuthenticationService> logger,
        IPasswordHasher<User> passwordHasher,
        IPasswordResetTokenDataProvider passwordResetTokenDataProvider,
        IRandomTokenGenerator randomTokenGenerator,
        ISecurityEventDataProvider securityEventDataProvider,
        IUserDataProvider userDataProvider)
    {
        this.authenticationMailer = authenticationMailer;
        this.clock = clock;
        this.dbContextFactory = dbContextFactory;
        this.logger = logger;
        this.passwordHasher = passwordHasher;
        this.passwordResetTokenDataProvider = passwordResetTokenDataProvider;
        this.randomTokenGenerator = randomTokenGenerator;
        this.securityEventDataProvider = securityEventDataProvider;
        this.userDataProvider = userDataProvider;
    }

    public async Task<User?> Authenticate(string email, string password, IPAddress? ipAddress)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        var user = await this.userDataProvider.FindUserByEmail(dbContext, email);

        if (user == null)
        {
            AuthenticateLogMessages.UnrecognizedEmail(this.logger, email, null);

            await this.securityEventDataProvider.LogEvent(
                dbContext, "authentication_failure:unrecognized_email", ipAddress);

            return null;
        }

        if (user.HashedPassword == null)
        {
            AuthenticateLogMessages.NoPasswordSet(this.logger, user.Id, user.Email, null);

            await this.securityEventDataProvider.LogEvent(
                dbContext, "authentication_failure:no_password_set", ipAddress, user.Id);

            return null;
        }

        if (!this.VerifyPassword(user, user.HashedPassword, password))
        {
            AuthenticateLogMessages.IncorrectPassword(this.logger, user.Id, user.Email, null);

            await this.securityEventDataProvider.LogEvent(
                dbContext, "authentication_failure:incorrect_password", ipAddress, user.Id);

            return null;
        }

        AuthenticateLogMessages.Success(this.logger, user.Id, user.Email, null);

        await this.securityEventDataProvider.LogEvent(
            dbContext, "authentication_success", ipAddress, user.Id);

        return user;
    }

    public async Task<bool> ChangePassword(
        long userId, string currentPassword, string newPassword, IPAddress? ipAddress)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        var user = await this.userDataProvider.GetUser(dbContext, userId);

        if (user.HashedPassword == null)
        {
            await this.securityEventDataProvider.LogEvent(
                dbContext, "password_change_failure:no_password_set", ipAddress, user.Id);

            throw new InvalidOperationException(
                $"User {user.Id} ({user.Email}) does not have a password.");
        }

        if (!this.VerifyPassword(user, user.HashedPassword, currentPassword))
        {
            ChangePasswordLogMessages.IncorrectPassword(
                this.logger, user.Id, user.Email, null);

            await this.securityEventDataProvider.LogEvent(
                dbContext, "password_change_failure:incorrect_password", ipAddress, user.Id);

            return false;
        }

        var newSecurityStamp = await this.SetPassword(dbContext, user, newPassword);

        ChangePasswordLogMessages.Success(this.logger, user.Id, user.Email, null);

        await this.securityEventDataProvider.LogEvent(
            dbContext, "password_change_success", ipAddress, user.Id);

        await this.authenticationMailer.SendPasswordChangeNotification(user.Email);

        return true;
    }

    public async Task<bool> PasswordResetTokenIsValid(string token, IPAddress? ipAddress)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        var userId = await this.ValidatePasswordResetToken(dbContext, token);

        if (userId.HasValue)
        {
            PasswordResetTokenIsValidLogMessages.Valid(
                this.logger, RedactToken(token), userId.Value, null);
        }
        else
        {
            PasswordResetTokenIsValidLogMessages.Invalid(this.logger, RedactToken(token), null);

            await this.securityEventDataProvider.LogEvent(
                dbContext, "password_reset_failure:invalid_token", ipAddress);
        }

        return userId.HasValue;
    }

    public async Task<User> ResetPassword(string token, string newPassword, IPAddress? ipAddress)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        var userId = await this.ValidatePasswordResetToken(dbContext, token);

        if (!userId.HasValue)
        {
            ResetPasswordLogMessages.InvalidToken(this.logger, RedactToken(token), null);

            await this.securityEventDataProvider.LogEvent(
                dbContext, "password_reset_failure:invalid_token", ipAddress);

            throw new InvalidTokenException("Password reset token is invalid");
        }

        var user = await this.userDataProvider.GetUser(dbContext, userId.Value);

        var newSecurityStamp = await this.SetPassword(dbContext, user, newPassword);

        ResetPasswordLogMessages.Success(this.logger, userId.Value, RedactToken(token), null);

        await this.securityEventDataProvider.LogEvent(
            dbContext, "password_reset_success", ipAddress, userId.Value);

        await this.authenticationMailer.SendPasswordChangeNotification(user.Email);

        return user with { SecurityStamp = newSecurityStamp };
    }

    public async Task SendPasswordResetLink(
        string email, IPAddress? ipAddress, IUrlHelper urlHelper)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        var user = await this.userDataProvider.FindUserByEmail(dbContext, email);

        if (user == null)
        {
            SendPasswordResetLinkLogMessages.UnrecognizedEmail(this.logger, email, null);

            await this.securityEventDataProvider.LogEvent(
                dbContext, "password_reset_failure:unrecognized_email", ipAddress);

            return;
        }

        email = user.Email;

        var token = this.randomTokenGenerator.Generate(12);

        await this.passwordResetTokenDataProvider.InsertToken(dbContext, user.Id, token);

        var link = urlHelper.Link("ResetPassword", new { token })!;

        await this.authenticationMailer.SendPasswordResetLink(email, link);

        SendPasswordResetLinkLogMessages.Success(this.logger, user.Id, email, null);

        await this.securityEventDataProvider.LogEvent(
            dbContext, "password_reset_link_sent", ipAddress, user.Id);
    }

    private static string RedactToken(string token) => $"{token[..6]}…";

    private async Task<string> SetPassword(AppDbContext dbContext, User user, string newPassword)
    {
        var hashedPassword = this.passwordHasher.HashPassword(user, newPassword);

        var securityStamp = this.randomTokenGenerator.Generate(2);

        await this.userDataProvider.UpdatePassword(
            dbContext, user.Id, hashedPassword, securityStamp);

        await this.passwordResetTokenDataProvider.DeleteTokensForUser(dbContext, user.Id);

        return securityStamp;
    }

    private async Task<long?> ValidatePasswordResetToken(AppDbContext dbContext, string token)
    {
        await this.passwordResetTokenDataProvider.DeleteExpiredTokens(
            dbContext, this.clock.UtcNow.AddDays(-1));

        return await this.passwordResetTokenDataProvider.GetUserIdForToken(dbContext, token);
    }

    private bool VerifyPassword(User user, string hashedPassword, string password) =>
        this.passwordHasher.VerifyHashedPassword(user, hashedPassword, password) !=
            PasswordVerificationResult.Failed;

    private static class AuthenticateLogMessages
    {
        public static readonly Action<ILogger, long, string, Exception?> IncorrectPassword =
            LoggerMessage.Define<long, string>(
                LogLevel.Information,
                200,
                "Authentication failed; incorrect password for user {UserId} ({Email})");

        public static readonly Action<ILogger, long, string, Exception?> NoPasswordSet =
            LoggerMessage.Define<long, string>(
                LogLevel.Information,
                201,
                "Authentication failed; no password set for user {UserId} ({Email})");

        public static readonly Action<ILogger, long, string, Exception?> Success =
            LoggerMessage.Define<long, string>(
                LogLevel.Information, 202, "User {UserId} ({Email}) successfully authenticated");

        public static readonly Action<ILogger, string, Exception?> UnrecognizedEmail =
            LoggerMessage.Define<string>(
                LogLevel.Information, 203, "Authentication failed; no user with email {Email}");
    }

    private static class ChangePasswordLogMessages
    {
        public static readonly Action<ILogger, long, string, Exception?> IncorrectPassword =
            LoggerMessage.Define<long, string>(
                LogLevel.Information,
                204,
                "Password change denied for user {UserId} ({Email}); current password is incorrect");

        public static readonly Action<ILogger, long, string, Exception?> Success =
            LoggerMessage.Define<long, string>(
                LogLevel.Information,
                205,
                "Password successfully changed for user {UserId} ({Email})");
    }

    private static class PasswordResetTokenIsValidLogMessages
    {
        public static readonly Action<ILogger, string, Exception?> Invalid =
            LoggerMessage.Define<string>(
                LogLevel.Debug, 206, "Password reset token '{Token}' is no longer valid");

        public static readonly Action<ILogger, string, long, Exception?> Valid =
            LoggerMessage.Define<string, long>(
                LogLevel.Debug,
                207,
                "Password reset token '{Token}' is valid and belongs to user {UserId}");
    }

    private static class ResetPasswordLogMessages
    {
        public static readonly Action<ILogger, string, Exception?> InvalidToken =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                208,
                "Unable to reset password; password reset token {Token} is invalid");

        public static readonly Action<ILogger, long, string, Exception?> Success =
            LoggerMessage.Define<long, string>(
                LogLevel.Information, 209, "Password reset for user {UserId} using token {Token}");
    }

    private static class SendPasswordResetLinkLogMessages
    {
        public static readonly Action<ILogger, long, string, Exception?> Success =
            LoggerMessage.Define<long, string>(
                LogLevel.Information, 210, "Password reset link sent to user {UserId} ({Email})");

        public static readonly Action<ILogger, string, Exception?> UnrecognizedEmail =
            LoggerMessage.Define<string>(
                LogLevel.Information,
                211,
                "Unable to send password reset link; No user with email {Email}");
    }
}
