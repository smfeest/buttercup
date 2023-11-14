using System.Net;
using Buttercup.EntityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Buttercup.Security;

internal sealed class PasswordAuthenticationService : IPasswordAuthenticationService
{
    private static readonly TimeSpan PasswordResetTokenExpiry = TimeSpan.FromDays(1);

    private readonly IAuthenticationMailer authenticationMailer;
    private readonly IClock clock;
    private readonly IDbContextFactory<AppDbContext> dbContextFactory;
    private readonly ILogger<PasswordAuthenticationService> logger;
    private readonly IPasswordHasher<User> passwordHasher;
    private readonly IRandomTokenGenerator randomTokenGenerator;

    public PasswordAuthenticationService(
        IAuthenticationMailer authenticationMailer,
        IClock clock,
        IDbContextFactory<AppDbContext> dbContextFactory,
        ILogger<PasswordAuthenticationService> logger,
        IPasswordHasher<User> passwordHasher,
        IRandomTokenGenerator randomTokenGenerator)
    {
        this.authenticationMailer = authenticationMailer;
        this.clock = clock;
        this.dbContextFactory = dbContextFactory;
        this.logger = logger;
        this.passwordHasher = passwordHasher;
        this.randomTokenGenerator = randomTokenGenerator;
    }

    public async Task<User?> Authenticate(string email, string password, IPAddress? ipAddress)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        var user = await FindByEmailAsync(dbContext.Users.AsTracking(), email);

        if (user == null)
        {
            await this.InsertSecurityEvent(
                dbContext, "authentication_failure:unrecognized_email", ipAddress);

            AuthenticateLogMessages.UnrecognizedEmail(this.logger, email, null);

            return null;
        }

        if (user.HashedPassword == null)
        {
            await this.InsertSecurityEvent(
                dbContext, "authentication_failure:no_password_set", ipAddress, user.Id);

            AuthenticateLogMessages.NoPasswordSet(this.logger, user.Id, user.Email, null);

            return null;
        }

        var verificationResult = this.passwordHasher.VerifyHashedPassword(
            user, user.HashedPassword, password);

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            await this.InsertSecurityEvent(
                dbContext, "authentication_failure:incorrect_password", ipAddress, user.Id);

            AuthenticateLogMessages.IncorrectPassword(this.logger, user.Id, user.Email, null);

            return null;
        }

        await this.InsertSecurityEvent(dbContext, "authentication_success", ipAddress, user.Id);

        AuthenticateLogMessages.Success(this.logger, user.Id, user.Email, null);

        if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            var unmodifiedUser = user with { };

            user.HashedPassword = this.passwordHasher.HashPassword(user, password);
            user.Modified = this.clock.UtcNow;
            user.Revision++;

            try
            {
                await dbContext.SaveChangesAsync();

                AuthenticateLogMessages.PasswordHashUpgraded(this.logger, user.Id, user.Email, null);
            }
            catch (DbUpdateConcurrencyException)
            {
                AuthenticateLogMessages.UpgradedPasswordHashNotPersisted(
                    this.logger, user.Id, user.Email, null);

                user = unmodifiedUser;
            }
        }

        return user;
    }

    public async Task<bool> ChangePassword(
        long userId, string currentPassword, string newPassword, IPAddress? ipAddress)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        var user = await dbContext.Users.AsTracking().GetAsync(userId);

        if (user.HashedPassword == null)
        {
            await this.InsertSecurityEvent(
                dbContext, "password_change_failure:no_password_set", ipAddress, user.Id);

            throw new InvalidOperationException(
                $"User {user.Id} ({user.Email}) does not have a password.");
        }

        var verificationResult = this.passwordHasher.VerifyHashedPassword(
            user, user.HashedPassword, currentPassword);

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            await this.InsertSecurityEvent(
                dbContext, "password_change_failure:incorrect_password", ipAddress, user.Id);

            ChangePasswordLogMessages.IncorrectPassword(this.logger, user.Id, user.Email, null);

            return false;
        }

        await this.SetPassword(dbContext, user, newPassword, "password_change_success", ipAddress);

        ChangePasswordLogMessages.Success(this.logger, user.Id, user.Email, null);

        await this.authenticationMailer.SendPasswordChangeNotification(user.Email);

        return true;
    }

    public async Task<bool> PasswordResetTokenIsValid(string token, IPAddress? ipAddress)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        var userId = await this.ValidPasswordResetToken(dbContext, token)
            .Select<PasswordResetToken, long?>(t => t.UserId)
            .FirstOrDefaultAsync();

        if (userId.HasValue)
        {
            PasswordResetTokenIsValidLogMessages.Valid(
                this.logger, RedactToken(token), userId.Value, null);
        }
        else
        {
            await this.InsertSecurityEvent(
                dbContext, "password_reset_failure:invalid_token", ipAddress);

            PasswordResetTokenIsValidLogMessages.Invalid(this.logger, RedactToken(token), null);
        }

        return userId.HasValue;
    }

    public async Task<User> ResetPassword(string token, string newPassword, IPAddress? ipAddress)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        var user = await this.ValidPasswordResetToken(dbContext, token)
            .AsTracking()
            .Select(t => t.User)
            .FirstOrDefaultAsync();

        if (user is null)
        {
            await this.InsertSecurityEvent(
                dbContext, "password_reset_failure:invalid_token", ipAddress);

            ResetPasswordLogMessages.InvalidToken(this.logger, RedactToken(token), null);

            throw new InvalidTokenException("Password reset token is invalid");
        }

        await this.SetPassword(dbContext, user, newPassword, "password_reset_success", ipAddress);

        ResetPasswordLogMessages.Success(this.logger, user.Id, RedactToken(token), null);

        await this.authenticationMailer.SendPasswordChangeNotification(user.Email);

        return user;
    }

    public async Task SendPasswordResetLink(
        string email, IPAddress? ipAddress, IUrlHelper urlHelper)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        var user = await FindByEmailAsync(dbContext.Users, email);

        if (user == null)
        {
            await this.InsertSecurityEvent(
                dbContext, "password_reset_failure:unrecognized_email", ipAddress);

            SendPasswordResetLinkLogMessages.UnrecognizedEmail(this.logger, email, null);

            return;
        }

        email = user.Email;

        var token = this.randomTokenGenerator.Generate(12);

        dbContext.PasswordResetTokens.Add(new()
        {
            Token = token,
            UserId = user.Id,
            Created = this.clock.UtcNow,
        });

        await dbContext.SaveChangesAsync();

        var link = urlHelper.Link("ResetPassword", new { token })!;

        await this.authenticationMailer.SendPasswordResetLink(email, link);

        await this.InsertSecurityEvent(dbContext, "password_reset_link_sent", ipAddress, user.Id);

        SendPasswordResetLinkLogMessages.Success(this.logger, user.Id, email, null);
    }

    private static Task<User?> FindByEmailAsync(IQueryable<User> queryable, string email) =>
        queryable.Where(u => u.Email == email).FirstOrDefaultAsync();

    private async Task InsertSecurityEvent(
        AppDbContext dbContext, string eventName, IPAddress? ipAddress, long? userId = null)
    {
        dbContext.SecurityEvents.Add(new()
        {
            Time = this.clock.UtcNow,
            Event = eventName,
            IpAddress = ipAddress,
            UserId = userId,
        });

        await dbContext.SaveChangesAsync();
    }

    private IQueryable<PasswordResetToken> ValidPasswordResetToken(
        AppDbContext dbContext, string token) =>
        dbContext.PasswordResetTokens.Where(t =>
            t.Created >= this.clock.UtcNow.Subtract(PasswordResetTokenExpiry) &&
            t.Token == token);

    private static string RedactToken(string token) => $"{token[..6]}…";

    private async Task SetPassword(
        AppDbContext dbContext,
        User user,
        string newPassword,
        string securityEventName,
        IPAddress? ipAddress)
    {
        var timestamp = this.clock.UtcNow;

        user.HashedPassword = this.passwordHasher.HashPassword(user, newPassword);
        user.SecurityStamp = this.randomTokenGenerator.Generate(2);
        user.PasswordCreated = timestamp;
        user.Modified = timestamp;
        user.Revision++;

        dbContext.SecurityEvents.Add(
            new()
            {
                Time = timestamp,
                Event = securityEventName,
                IpAddress = ipAddress,
                UserId = user.Id
            });

        await dbContext.SaveChangesAsync();

        await dbContext.PasswordResetTokens.Where(t => t.User == user).ExecuteDeleteAsync();
    }

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

        public static readonly Action<ILogger, long, string, Exception?> PasswordHashUpgraded =
            LoggerMessage.Define<long, string>(
                LogLevel.Information, 217, "Password hash upgraded for user {UserId} ({Email})");

        public static readonly Action<ILogger, long, string, Exception?> UpgradedPasswordHashNotPersisted =
            LoggerMessage.Define<long, string>(
                LogLevel.Information,
                218,
                "Upgraded password hash not persisted for user {UserId} ({Email}); concurrent changed detected");

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
