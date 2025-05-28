using System.Net;
using Buttercup.EntityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Buttercup.Security;

internal sealed partial class PasswordAuthenticationService(
    IAuthenticationMailer authenticationMailer,
    IDbContextFactory<AppDbContext> dbContextFactory,
    ILogger<PasswordAuthenticationService> logger,
    IPasswordAuthenticationRateLimiter passwordAuthenticationRateLimiter,
    IPasswordHasher<User> passwordHasher,
    IPasswordResetRateLimiter passwordResetRateLimiter,
    IRandomTokenGenerator randomTokenGenerator,
    TimeProvider timeProvider)
    : IPasswordAuthenticationService
{
    private static readonly TimeSpan PasswordResetTokenExpiry = TimeSpan.FromDays(1);

    private readonly IAuthenticationMailer authenticationMailer = authenticationMailer;
    private readonly IDbContextFactory<AppDbContext> dbContextFactory = dbContextFactory;
    private readonly ILogger<PasswordAuthenticationService> logger = logger;
    private readonly IPasswordAuthenticationRateLimiter passwordAuthenticationRateLimiter =
        passwordAuthenticationRateLimiter;
    private readonly IPasswordHasher<User> passwordHasher = passwordHasher;
    private readonly IPasswordResetRateLimiter passwordResetRateLimiter = passwordResetRateLimiter;
    private readonly IRandomTokenGenerator randomTokenGenerator = randomTokenGenerator;
    private readonly TimeProvider timeProvider = timeProvider;

    public async Task<PasswordAuthenticationResult> Authenticate(
        string email, string password, IPAddress? ipAddress)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        if (!await this.passwordAuthenticationRateLimiter.IsAllowed(email))
        {
            await this.InsertSecurityEvent(
                dbContext, "authentication_failure:rate_limit_exceeded", ipAddress);

            this.LogAuthenticationFailedRateLimitExceeded(email);

            return new(PasswordAuthenticationFailure.TooManyAttempts);
        }

        var user = await FindByEmailAsync(dbContext.Users.AsTracking(), email);

        if (user == null)
        {
            await this.InsertSecurityEvent(
                dbContext, "authentication_failure:unrecognized_email", ipAddress);

            this.LogAuthenticationFailedUnrecognizedEmail(email);

            return new(PasswordAuthenticationFailure.IncorrectCredentials);
        }

        if (user.HashedPassword == null)
        {
            await this.InsertSecurityEvent(
                dbContext, "authentication_failure:no_password_set", ipAddress, user.Id);

            this.LogAuthenticationFailedNoPasswordSet(user.Id, user.Email);

            return new(PasswordAuthenticationFailure.IncorrectCredentials);
        }

        var verificationResult = this.passwordHasher.VerifyHashedPassword(
            user, user.HashedPassword, password);

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            await this.InsertSecurityEvent(
                dbContext, "authentication_failure:incorrect_password", ipAddress, user.Id);

            this.LogAuthenticationFailedIncorrectPassword(user.Id, user.Email);

            return new(PasswordAuthenticationFailure.IncorrectCredentials);
        }

        await this.InsertSecurityEvent(dbContext, "authentication_success", ipAddress, user.Id);

        this.LogAuthenticated(user.Id, user.Email);

        await this.passwordAuthenticationRateLimiter.Reset(user.Email);

        if (verificationResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            var unmodifiedUser = user with { };

            user.HashedPassword = this.passwordHasher.HashPassword(user, password);
            user.Modified = this.timeProvider.GetUtcDateTimeNow();
            user.Revision++;

            try
            {
                await dbContext.SaveChangesAsync();

                this.LogPasswordHashUpgraded(user.Id, user.Email);
            }
            catch (DbUpdateConcurrencyException)
            {
                this.LogUpgradedPasswordHashNotPersisted(user.Id, user.Email);

                user = unmodifiedUser;
            }
        }

        return new(user);
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

            this.LogPasswordChangeFailedIncorrectPassword(user.Id, user.Email);

            return false;
        }

        await this.SetPassword(dbContext, user, newPassword, "password_change_success", ipAddress);

        this.LogPasswordChanged(user.Id, user.Email);

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
            this.LogPasswordResetTokenValid(RedactToken(token), userId.Value);
        }
        else
        {
            await this.InsertSecurityEvent(
                dbContext, "password_reset_failure:invalid_token", ipAddress);

            this.LogPasswordResetTokenInvalid(RedactToken(token));
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

            this.LogPasswordResetFailedInvalidToken(RedactToken(token));

            throw new InvalidTokenException("Password reset token is invalid");
        }

        await this.SetPassword(dbContext, user, newPassword, "password_reset_success", ipAddress);

        this.LogPasswordReset(user.Id, RedactToken(token));

        await this.authenticationMailer.SendPasswordChangeNotification(user.Email);

        await this.passwordAuthenticationRateLimiter.Reset(user.Email);

        return user;
    }

    public async Task<bool> SendPasswordResetLink(
        string email, IPAddress? ipAddress, IUrlHelper urlHelper)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        if (!await this.passwordResetRateLimiter.IsAllowed(email))
        {
            await this.InsertSecurityEvent(
                dbContext, "password_reset_failure:rate_limit_exceeded", ipAddress);

            this.LogPasswordResetLinkNotSentRateLimitExceeded(email);

            return false;
        }

        var user = await FindByEmailAsync(dbContext.Users, email);

        if (user == null)
        {
            await this.InsertSecurityEvent(
                dbContext, "password_reset_failure:unrecognized_email", ipAddress);

            this.LogPasswordResetLinkNotSentUnrecognizedEmail(email);

            return true;
        }

        email = user.Email;

        var token = this.randomTokenGenerator.Generate(12);

        dbContext.PasswordResetTokens.Add(new()
        {
            Token = token,
            UserId = user.Id,
            Created = this.timeProvider.GetUtcDateTimeNow(),
        });

        await dbContext.SaveChangesAsync();

        var link = urlHelper.Link("ResetPassword", new { token })!;

        await this.authenticationMailer.SendPasswordResetLink(email, link);

        await this.InsertSecurityEvent(dbContext, "password_reset_link_sent", ipAddress, user.Id);

        this.LogPasswordResetLinkSent(user.Id, email);

        return true;
    }

    private static Task<User?> FindByEmailAsync(IQueryable<User> queryable, string email) =>
        queryable.Where(u => u.Email == email).FirstOrDefaultAsync();

    private async Task InsertSecurityEvent(
        AppDbContext dbContext, string eventName, IPAddress? ipAddress, long? userId = null)
    {
        dbContext.SecurityEvents.Add(new()
        {
            Time = this.timeProvider.GetUtcDateTimeNow(),
            Event = eventName,
            IpAddress = ipAddress,
            UserId = userId,
        });

        await dbContext.SaveChangesAsync();
    }

    private IQueryable<PasswordResetToken> ValidPasswordResetToken(
        AppDbContext dbContext, string token) =>
        dbContext.PasswordResetTokens.Where(t =>
            t.Created >= this.timeProvider.GetUtcDateTimeNow().Subtract(PasswordResetTokenExpiry) &&
            t.Token == token);

    private static string RedactToken(string token) => $"{token[..6]}…";

    private async Task SetPassword(
        AppDbContext dbContext,
        User user,
        string newPassword,
        string securityEventName,
        IPAddress? ipAddress)
    {
        var timestamp = this.timeProvider.GetUtcDateTimeNow();

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

    [LoggerMessage(
        EventId = 1,
        EventName = "Authenticated",
        Level = LogLevel.Information,
        Message = "User {UserId} ({Email}) successfully authenticated")]
    private partial void LogAuthenticated(long userId, string email);

    [LoggerMessage(
        EventId = 2,
        EventName = "AuthenticationFailedIncorrectPassword",
        Level = LogLevel.Information,
        Message = "Authentication failed; incorrect password for user {UserId} ({Email})")]
    private partial void LogAuthenticationFailedIncorrectPassword(long userId, string email);

    [LoggerMessage(
        EventId = 3,
        EventName = "AuthenticationFailedNoPasswordSet",
        Level = LogLevel.Information,
        Message = "Authentication failed; no password set for user {UserId} ({Email})")]
    private partial void LogAuthenticationFailedNoPasswordSet(long userId, string email);

    [LoggerMessage(
        EventId = 4,
        EventName = "AuthenticationFailedRateLimitExceeded",
        Level = LogLevel.Information,
        Message = "Authentication failed; rate limit exceeded for email {Email}")]
    private partial void LogAuthenticationFailedRateLimitExceeded(string email);

    [LoggerMessage(
        EventId = 5,
        EventName = "AuthenticationFailedUnrecognizedEmail",
        Level = LogLevel.Information,
        Message = "Authentication failed; no user with email {Email}")]
    private partial void LogAuthenticationFailedUnrecognizedEmail(string email);

    [LoggerMessage(
        EventId = 6,
        EventName = "PasswordChanged",
        Level = LogLevel.Information,
        Message = "Password successfully changed for user {UserId} ({Email})")]
    private partial void LogPasswordChanged(long userId, string email);

    [LoggerMessage(
        EventId = 7,
        EventName = "PasswordChangeFailedIncorrectPassword",
        Level = LogLevel.Information,
        Message = "Password change denied for user {UserId} ({Email}); current password is incorrect")]
    private partial void LogPasswordChangeFailedIncorrectPassword(long userId, string email);

    [LoggerMessage(
        EventId = 8,
        EventName = "PasswordHashUpgraded",
        Level = LogLevel.Information,
        Message = "Password hash upgraded for user {UserId} ({Email})")]
    private partial void LogPasswordHashUpgraded(long userId, string email);

    [LoggerMessage(
        EventId = 9,
        EventName = "PasswordReset",
        Level = LogLevel.Information,
        Message = "Password reset for user {UserId} using token {Token}")]
    private partial void LogPasswordReset(long userId, string token);

    [LoggerMessage(
        EventId = 10,
        EventName = "PasswordResetFailedInvalidToken",
        Level = LogLevel.Information,
        Message = "Unable to reset password; password reset token {Token} is invalid")]
    private partial void LogPasswordResetFailedInvalidToken(string token);

    [LoggerMessage(
        EventId = 11,
        EventName = "PasswordResetLinkNotSentRateLimitExceeded",
        Level = LogLevel.Information,
        Message = "Unable to send password reset link to {Email}; rate limit exceeded")]
    private partial void LogPasswordResetLinkNotSentRateLimitExceeded(string email);

    [LoggerMessage(
        EventId = 12,
        EventName = "PasswordResetLinkNotSentUnrecognizedEmail",
        Level = LogLevel.Information,
        Message = "Unable to send password reset link to {Email}; no matching user")]
    private partial void LogPasswordResetLinkNotSentUnrecognizedEmail(string email);

    [LoggerMessage(
        EventId = 13,
        EventName = "PasswordResetLinkSent",
        Level = LogLevel.Information,
        Message = "Password reset link sent to user {UserId} ({Email})")]
    private partial void LogPasswordResetLinkSent(long userId, string email);

    [LoggerMessage(
        EventId = 14,
        EventName = "PasswordResetTokenInvalid",
        Level = LogLevel.Debug,
        Message = "Password reset token '{Token}' is no longer valid")]
    private partial void LogPasswordResetTokenInvalid(string token);

    [LoggerMessage(
        EventId = 15,
        EventName = "PasswordResetTokenValid",
        Level = LogLevel.Debug,
        Message = "Password reset token '{Token}' is valid and belongs to user {UserId}")]
    private partial void LogPasswordResetTokenValid(string token, long userId);

    [LoggerMessage(
        EventId = 16,
        EventName = "UpgradedPasswordHashNotPersisted",
        Level = LogLevel.Information,
        Message = "Upgraded password hash not persisted for user {UserId} ({Email}); concurrent changed detected")]
    private partial void LogUpgradedPasswordHashNotPersisted(long userId, string email);
}
