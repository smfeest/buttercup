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
    IParameterMaskingService parameterMaskingService,
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
    private readonly IParameterMaskingService parameterMaskingService = parameterMaskingService;
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
            this.LogAuthenticationFailedRateLimitExceeded(email);

            return new(PasswordAuthenticationFailure.TooManyAttempts);
        }

        var user = await FindByEmailAsync(dbContext.Users.AsTracking(), email);

        if (user == null)
        {
            this.LogAuthenticationFailedUnrecognizedEmail(email);

            return new(PasswordAuthenticationFailure.IncorrectCredentials);
        }

        if (user.Deactivated.HasValue)
        {
            dbContext.UserAuditEntries.Add(
                new()
                {
                    Time = this.timeProvider.GetUtcDateTimeNow(),
                    Operation = UserAuditOperation.AuthenticatePassword,
                    TargetId = user.Id,
                    ActorId = user.Id,
                    IpAddress = ipAddress,
                    Failure = UserAuditFailure.UserDeactivated,
                });
            await dbContext.SaveChangesAsync();

            this.LogAuthenticationFailedUserDeactivated(user.Id, user.Email);

            return new(PasswordAuthenticationFailure.IncorrectCredentials);
        }

        if (user.HashedPassword == null)
        {
            dbContext.UserAuditEntries.Add(
                new()
                {
                    Time = this.timeProvider.GetUtcDateTimeNow(),
                    Operation = UserAuditOperation.AuthenticatePassword,
                    TargetId = user.Id,
                    ActorId = user.Id,
                    IpAddress = ipAddress,
                    Failure = UserAuditFailure.NoPasswordSet,
                });
            await dbContext.SaveChangesAsync();

            this.LogAuthenticationFailedNoPasswordSet(user.Id, user.Email);

            return new(PasswordAuthenticationFailure.IncorrectCredentials);
        }

        var verificationResult = this.passwordHasher.VerifyHashedPassword(
            user, user.HashedPassword, password);

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            dbContext.UserAuditEntries.Add(
                new()
                {
                    Time = this.timeProvider.GetUtcDateTimeNow(),
                    Operation = UserAuditOperation.AuthenticatePassword,
                    TargetId = user.Id,
                    ActorId = user.Id,
                    IpAddress = ipAddress,
                    Failure = UserAuditFailure.IncorrectPassword,
                });
            await dbContext.SaveChangesAsync();

            this.LogAuthenticationFailedIncorrectPassword(user.Id, user.Email);

            return new(PasswordAuthenticationFailure.IncorrectCredentials);
        }

        dbContext.UserAuditEntries.Add(
            new()
            {
                Time = this.timeProvider.GetUtcDateTimeNow(),
                Operation = UserAuditOperation.AuthenticatePassword,
                TargetId = user.Id,
                ActorId = user.Id,
                IpAddress = ipAddress,
            });
        await dbContext.SaveChangesAsync();

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

    public async Task<PasswordResetResult> CanResetPassword(string token, IPAddress? ipAddress)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        var user = await this.ValidPasswordResetToken(dbContext, token)
            .Select(t => t.User)
            .FirstOrDefaultAsync();

        var maskedToken = this.parameterMaskingService.MaskToken(token);

        if (user is null)
        {
            this.LogCannotResetPasswordTokenInvalid(maskedToken);

            return new(PasswordResetFailure.InvalidToken);
        }

        if (user.Deactivated.HasValue)
        {
            dbContext.UserAuditEntries.Add(
                new()
                {
                    Time = this.timeProvider.GetUtcDateTimeNow(),
                    Operation = UserAuditOperation.ResetPassword,
                    TargetId = user.Id,
                    ActorId = user.Id,
                    IpAddress = ipAddress,
                    Failure = UserAuditFailure.UserDeactivated,
                });
            await dbContext.SaveChangesAsync();

            this.LogCannotResetPasswordUserDeactivated(maskedToken, user.Id, user.Email);

            return new(PasswordResetFailure.UserDeactivated);
        }

        this.LogCanResetPassword(maskedToken, user.Id, user.Email);

        return new(user);
    }

    public async Task<bool> ChangePassword(
        long userId, string currentPassword, string newPassword, IPAddress? ipAddress)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        var user = await dbContext.Users.AsTracking().GetAsync(userId);

        if (user.HashedPassword == null)
        {
            dbContext.UserAuditEntries.Add(
                new()
                {
                    Time = this.timeProvider.GetUtcDateTimeNow(),
                    Operation = UserAuditOperation.ChangePassword,
                    TargetId = user.Id,
                    ActorId = user.Id,
                    IpAddress = ipAddress,
                    Failure = UserAuditFailure.NoPasswordSet,
                });
            await dbContext.SaveChangesAsync();

            this.LogPasswordChangeFailedNoPasswordSet(user.Id, user.Email);

            throw new InvalidOperationException(
                $"User {user.Id} ({user.Email}) does not have a password.");
        }

        var verificationResult = this.passwordHasher.VerifyHashedPassword(
            user, user.HashedPassword, currentPassword);

        if (verificationResult == PasswordVerificationResult.Failed)
        {
            dbContext.UserAuditEntries.Add(
                new()
                {
                    Time = this.timeProvider.GetUtcDateTimeNow(),
                    Operation = UserAuditOperation.ChangePassword,
                    TargetId = user.Id,
                    ActorId = user.Id,
                    IpAddress = ipAddress,
                    Failure = UserAuditFailure.IncorrectPassword,
                });
            await dbContext.SaveChangesAsync();

            this.LogPasswordChangeFailedIncorrectPassword(user.Id, user.Email);

            return false;
        }

        await this.SetPassword(
            dbContext, user, newPassword, UserAuditOperation.ChangePassword, ipAddress);

        this.LogPasswordChanged(user.Id, user.Email);

        await this.authenticationMailer.SendPasswordChangeNotification(user.Email);

        return true;
    }

    public async Task<PasswordResetResult> ResetPassword(
        string token, string newPassword, IPAddress? ipAddress)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        var user = await this.ValidPasswordResetToken(dbContext, token)
            .AsTracking()
            .Select(t => t.User)
            .FirstOrDefaultAsync();

        var maskedToken = this.parameterMaskingService.MaskToken(token);

        if (user is null)
        {
            this.LogPasswordResetFailedInvalidToken(maskedToken);

            return new(PasswordResetFailure.InvalidToken);
        }

        if (user.Deactivated.HasValue)
        {
            dbContext.UserAuditEntries.Add(
                new()
                {
                    Time = this.timeProvider.GetUtcDateTimeNow(),
                    Operation = UserAuditOperation.ResetPassword,
                    TargetId = user.Id,
                    ActorId = user.Id,
                    IpAddress = ipAddress,
                    Failure = UserAuditFailure.UserDeactivated,
                });
            await dbContext.SaveChangesAsync();

            this.LogPasswordResetFailedUserDeactivated(maskedToken, user.Id, user.Email);

            return new(PasswordResetFailure.UserDeactivated);
        }

        await this.SetPassword(
            dbContext, user, newPassword, UserAuditOperation.ResetPassword, ipAddress);

        this.LogPasswordReset(user.Id, maskedToken);

        await this.authenticationMailer.SendPasswordChangeNotification(user.Email);

        await this.passwordAuthenticationRateLimiter.Reset(user.Email);

        return new(user);
    }

    public async Task<bool> SendPasswordResetLink(
        string email, IPAddress? ipAddress, IUrlHelper urlHelper)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        if (!await this.passwordResetRateLimiter.IsAllowed(email))
        {
            this.LogPasswordResetLinkNotSentRateLimitExceeded(email);

            return false;
        }

        var user = await FindByEmailAsync(dbContext.Users, email);

        if (user == null)
        {
            this.LogPasswordResetLinkNotSentUnrecognizedEmail(email);

            return true;
        }

        email = user.Email;

        var token = this.randomTokenGenerator.Generate(12);

        dbContext.PasswordResetTokens.Add(
            new()
            {
                Token = token,
                UserId = user.Id,
                Created = this.timeProvider.GetUtcDateTimeNow(),
            });
        dbContext.UserAuditEntries.Add(
            new()
            {
                Time = this.timeProvider.GetUtcDateTimeNow(),
                Operation = UserAuditOperation.CreatePasswordResetToken,
                TargetId = user.Id,
                ActorId = user.Id,
                IpAddress = ipAddress,
            });

        await dbContext.SaveChangesAsync();

        var link = urlHelper.Link("ResetPassword", new { token })!;

        await this.authenticationMailer.SendPasswordResetLink(email, link);

        this.LogPasswordResetLinkSent(user.Id, email);

        return true;
    }

    private static Task<User?> FindByEmailAsync(IQueryable<User> queryable, string email) =>
        queryable.Where(u => u.Email == email).FirstOrDefaultAsync();

    private IQueryable<PasswordResetToken> ValidPasswordResetToken(
        AppDbContext dbContext, string token) =>
        dbContext.PasswordResetTokens.Where(t =>
            t.Created >= this.timeProvider.GetUtcDateTimeNow().Subtract(PasswordResetTokenExpiry) &&
            t.Token == token);

    private async Task SetPassword(
        AppDbContext dbContext,
        User user,
        string newPassword,
        UserAuditOperation operation,
        IPAddress? ipAddress)
    {
        var timestamp = this.timeProvider.GetUtcDateTimeNow();

        user.HashedPassword = this.passwordHasher.HashPassword(user, newPassword);
        user.SecurityStamp = this.randomTokenGenerator.Generate(2);
        user.PasswordCreated = timestamp;
        user.Modified = timestamp;
        user.Revision++;

        dbContext.UserAuditEntries.Add(
            new()
            {
                Time = timestamp,
                Operation = operation,
                TargetId = user.Id,
                ActorId = user.Id,
                IpAddress = ipAddress
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
        EventId = 17,
        EventName = "AuthenticationFailedUserDeactivated",
        Level = LogLevel.Information,
        Message = "Authentication failed; user {UserId} ({Email}) is deactivated")]
    private partial void LogAuthenticationFailedUserDeactivated(long userId, string email);

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
        EventId = 14,
        EventName = "CannotResetPasswordTokenInvalid",
        Level = LogLevel.Information,
        Message = "Cannot use token '{Token}' to reset password; token is invalid")]
    private partial void LogCannotResetPasswordTokenInvalid(string token);

    [LoggerMessage(
        EventId = 18,
        EventName = "CannotResetPasswordUserDeactivated",
        Level = LogLevel.Information,
        Message = "Cannot use token '{Token}' to reset password; user {UserId} ({Email}) is deactivated")]
    private partial void LogCannotResetPasswordUserDeactivated(
        string token, long userId, string email);

    [LoggerMessage(
        EventId = 15,
        EventName = "CanResetPassword",
        Level = LogLevel.Debug,
        Message = "Can use token '{Token}' to reset password for user {UserId} ({Email})")]
    private partial void LogCanResetPassword(string token, long userId, string email);

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
        EventId = 20,
        EventName = "PasswordChangeFailedNoPasswordSet",
        Level = LogLevel.Information,
        Message = "Password change denied; no password set for user {UserId} ({Email})")]
    private partial void LogPasswordChangeFailedNoPasswordSet(long userId, string email);

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
        EventId = 19,
        EventName = "PasswordResetFailedUserDeactivated",
        Level = LogLevel.Information,
        Message = "Unable to reset password using token {Token}; user {UserId} ({Email}) is deactivated")]
    private partial void LogPasswordResetFailedUserDeactivated(
        string token, long userId, string email);

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
        EventId = 16,
        EventName = "UpgradedPasswordHashNotPersisted",
        Level = LogLevel.Information,
        Message = "Upgraded password hash not persisted for user {UserId} ({Email}); concurrent changed detected")]
    private partial void LogUpgradedPasswordHashNotPersisted(long userId, string email);
}
