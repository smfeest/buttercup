using System.Globalization;
using System.Net;
using Buttercup.EntityModel;
using Buttercup.TestUtils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Xunit;

namespace Buttercup.Security;

[Collection(nameof(DatabaseCollection))]
public sealed class PasswordAuthenticationServiceTests : DatabaseTests<DatabaseCollection>
{
    private readonly ModelFactory modelFactory = new();

    private readonly Mock<IAuthenticationMailer> authenticationMailerMock = new();
    private readonly FakeLogger<PasswordAuthenticationService> logger = new();
    private readonly Mock<IParameterMaskingService> parameterMaskingServiceMock = new();
    private readonly Mock<IPasswordAuthenticationRateLimiter> passwordAuthenticationRateLimiterMock
        = new();
    private readonly Mock<IPasswordHasher<User>> passwordHasherMock = new();
    private readonly Mock<IPasswordResetRateLimiter> passwordResetRateLimiterMock = new();
    private readonly Mock<IRandomTokenGenerator> randomTokenGeneratorMock = new();
    private readonly FakeTimeProvider timeProvider;

    private readonly PasswordAuthenticationService passwordAuthenticationService;

    public PasswordAuthenticationServiceTests(DatabaseFixture<DatabaseCollection> databaseFixture)
        : base(databaseFixture)
    {
        this.timeProvider = new(this.modelFactory.NextDateTime());

        this.passwordAuthenticationService = new(
            this.authenticationMailerMock.Object,
            this.DatabaseFixture,
            this.logger,
            this.parameterMaskingServiceMock.Object,
            this.passwordAuthenticationRateLimiterMock.Object,
            this.passwordHasherMock.Object,
            this.passwordResetRateLimiterMock.Object,
            this.randomTokenGeneratorMock.Object,
            this.timeProvider);
    }

    #region Authenticate

    [Fact]
    public async Task Authenticate_RateLimitExceeded()
    {
        var email = this.modelFactory.NextEmail();
        var maskedEmail = this.SetupMaskEmail(email);
        var password = this.modelFactory.NextString("password");
        var ipAddress = this.modelFactory.NextIpAddress();

        this.SetPasswordAuthenticationRateLimiterResult(email, false);

        var result = await this.passwordAuthenticationService.Authenticate(
            email, password, ipAddress);

        using var dbContext = this.DatabaseFixture.CreateDbContext();

        // Logs rate limit exceeded message
        LogAssert.SingleEntry(this.logger)
            .HasId(4)
            .HasLevel(LogLevel.Information)
            .HasMessage($"Authentication failed; rate limit exceeded for email {maskedEmail}");

        // Returns 'too many requests' failure
        Assert.Equal(PasswordAuthenticationFailure.TooManyAttempts, result.Failure);
    }

    [Fact]
    public async Task Authenticate_EmailUnrecognized()
    {
        var email = this.modelFactory.NextEmail();
        var maskedEmail = this.SetupMaskEmail(email);
        var password = this.modelFactory.NextString("password");
        var ipAddress = this.modelFactory.NextIpAddress();

        await this.DatabaseFixture.InsertEntities(this.modelFactory.BuildUser());

        this.SetPasswordAuthenticationRateLimiterResult(email, true);

        var result = await this.passwordAuthenticationService.Authenticate(
            email, password, ipAddress);

        using var dbContext = this.DatabaseFixture.CreateDbContext();

        // Logs unrecognized email message
        LogAssert.SingleEntry(this.logger)
            .HasId(5)
            .HasLevel(LogLevel.Information)
            .HasMessage($"Authentication failed; no user with email {maskedEmail}");

        // Returns 'incorrect credentials' failure
        Assert.Equal(PasswordAuthenticationFailure.IncorrectCredentials, result.Failure);
    }

    [Fact]
    public async Task Authenticate_UserDeactivated()
    {
        var password = this.modelFactory.NextString("password");
        var ipAddress = this.modelFactory.NextIpAddress();

        var user = this.modelFactory.BuildUser(deactivated: true);
        await this.DatabaseFixture.InsertEntities(user);

        this.SetPasswordAuthenticationRateLimiterResult(user.Email, true);

        var result = await this.passwordAuthenticationService.Authenticate(
            user.Email, password, ipAddress);

        using var dbContext = this.DatabaseFixture.CreateDbContext();

        // Inserts user audit entry
        await this.AssertSingleUserAuditEntry(
            dbContext,
            UserAuditOperation.AuthenticatePassword,
            user.Id,
            user.Id,
            ipAddress,
            UserAuditFailure.UserDeactivated);

        // Logs user deactivated message
        LogAssert.SingleEntry(this.logger)
            .HasId(17)
            .HasLevel(LogLevel.Information)
            .HasMessage(
                $"Authentication failed; user {user.Id} ({user.Email}) is deactivated");

        // Returns 'incorrect credentials' failure
        Assert.Equal(PasswordAuthenticationFailure.IncorrectCredentials, result.Failure);
    }

    [Fact]
    public async Task Authenticate_UserHasNoPassword()
    {
        var password = this.modelFactory.NextString("password");
        var ipAddress = this.modelFactory.NextIpAddress();

        var user = this.modelFactory.BuildUser() with { HashedPassword = null };
        await this.DatabaseFixture.InsertEntities(user);

        this.SetPasswordAuthenticationRateLimiterResult(user.Email, true);

        var result = await this.passwordAuthenticationService.Authenticate(
            user.Email, password, ipAddress);

        using var dbContext = this.DatabaseFixture.CreateDbContext();

        // Inserts user audit entry
        await this.AssertSingleUserAuditEntry(
            dbContext,
            UserAuditOperation.AuthenticatePassword,
            user.Id,
            user.Id,
            ipAddress,
            UserAuditFailure.NoPasswordSet);

        // Logs no password set message
        LogAssert.SingleEntry(this.logger)
            .HasId(3)
            .HasLevel(LogLevel.Information)
            .HasMessage(
                $"Authentication failed; no password set for user {user.Id} ({user.Email})");

        // Returns 'incorrect credentials' failure
        Assert.Equal(PasswordAuthenticationFailure.IncorrectCredentials, result.Failure);
    }

    [Fact]
    public async Task Authenticate_IncorrectPassword()
    {
        var password = this.modelFactory.NextString("password");
        var hashedPassword = this.modelFactory.NextString("hashed-password");
        var ipAddress = this.modelFactory.NextIpAddress();

        var user = this.modelFactory.BuildUser() with { HashedPassword = hashedPassword };
        await this.DatabaseFixture.InsertEntities(user);

        this.SetPasswordAuthenticationRateLimiterResult(user.Email, true);
        this.SetupVerifyHashedPassword(
            user, hashedPassword, password, PasswordVerificationResult.Failed);

        var result = await this.passwordAuthenticationService.Authenticate(
            user.Email, password, ipAddress);

        using var dbContext = this.DatabaseFixture.CreateDbContext();

        // Inserts user audit entry
        await this.AssertSingleUserAuditEntry(
            dbContext,
            UserAuditOperation.AuthenticatePassword,
            user.Id,
            user.Id,
            ipAddress,
            UserAuditFailure.IncorrectPassword);

        // Logs incorrect password message
        LogAssert.SingleEntry(this.logger)
            .HasId(2)
            .HasLevel(LogLevel.Information)
            .HasMessage(
                $"Authentication failed; incorrect password for user {user.Id} ({user.Email})");

        // Returns 'incorrect credentials' failure
        Assert.Equal(PasswordAuthenticationFailure.IncorrectCredentials, result.Failure);
    }

    [Fact]
    public async Task Authenticate_SuccessRehashNotNeeded()
    {
        var password = this.modelFactory.NextString("password");
        var hashedPassword = this.modelFactory.NextString("hashed-password");
        var ipAddress = this.modelFactory.NextIpAddress();

        var user = this.modelFactory.BuildUser() with { HashedPassword = hashedPassword };
        await this.DatabaseFixture.InsertEntities(user);

        this.SetPasswordAuthenticationRateLimiterResult(user.Email, true);
        this.SetupVerifyHashedPassword(
            user, hashedPassword, password, PasswordVerificationResult.Success);

        var result = await this.passwordAuthenticationService.Authenticate(
            user.Email, password, ipAddress);

        using var dbContext = this.DatabaseFixture.CreateDbContext();

        // Inserts user audit entry
        await this.AssertSingleUserAuditEntry(
            dbContext, UserAuditOperation.AuthenticatePassword, user.Id, user.Id, ipAddress);

        // Resets the rate limit counters
        this.passwordAuthenticationRateLimiterMock.Verify(x => x.Reset(user.Email));

        // Logs successfully authenticated message
        LogAssert.SingleEntry(this.logger)
            .HasId(1)
            .HasLevel(LogLevel.Information)
            .HasMessage($"User {user.Id} ({user.Email}) successfully authenticated");

        // Does not rehash password
        this.passwordHasherMock.Verify(x => x.HashPassword(user, password), Times.Never);

        // Returns user
        Assert.Equal(user, result.User);
    }

    [Fact]
    public async Task Authenticate_SuccessRehashNeeded()
    {
        var password = this.modelFactory.NextString("password");
        var hashedPassword = this.modelFactory.NextString("hashed-password");
        var ipAddress = this.modelFactory.NextIpAddress();

        var userBefore = this.modelFactory.BuildUser() with
        {
            HashedPassword = hashedPassword,
            PasswordCreated = this.modelFactory.NextDateTime(),
        };
        await this.DatabaseFixture.InsertEntities(userBefore);

        this.SetPasswordAuthenticationRateLimiterResult(userBefore.Email, true);
        this.SetupVerifyHashedPassword(
            userBefore,
            hashedPassword,
            password,
            PasswordVerificationResult.SuccessRehashNeeded);
        var rehashedPassword = this.SetupHashPassword(userBefore, password);

        var result = await this.passwordAuthenticationService.Authenticate(
            userBefore.Email, password, ipAddress);

        using var dbContext = this.DatabaseFixture.CreateDbContext();

        // Inserts user audit entry
        await this.AssertSingleUserAuditEntry(
            dbContext,
            UserAuditOperation.AuthenticatePassword,
            userBefore.Id,
            userBefore.Id,
            ipAddress);

        // Logs successfully authenticated message
        LogAssert.SingleEntry(this.logger, 1)
            .HasLevel(LogLevel.Information)
            .HasMessage($"User {userBefore.Id} ({userBefore.Email}) successfully authenticated");

        var expectedUserAfter = userBefore with
        {
            HashedPassword = rehashedPassword,
            Modified = this.timeProvider.GetUtcDateTimeNow(),
            Revision = userBefore.Revision + 1,
        };

        // Updates user in database
        Assert.Equal(
            expectedUserAfter,
            await dbContext.Users.FindAsync(
                [userBefore.Id], TestContext.Current.CancellationToken));

        // Logs password hash upgraded message
        LogAssert.SingleEntry(this.logger, 8)
            .HasLevel(LogLevel.Information)
            .HasMessage($"Password hash upgraded for user {userBefore.Id} ({userBefore.Email})");

        // Returns updated user
        Assert.Equal(expectedUserAfter, result.User);
    }

    [Fact]
    public async Task Authenticate_PasswordRehashNotPersisted()
    {
        var password = this.modelFactory.NextString("password");
        var hashedPassword = this.modelFactory.NextString("hashed-password");
        var ipAddress = this.modelFactory.NextIpAddress();

        var userBefore = this.modelFactory.BuildUser() with
        {
            HashedPassword = hashedPassword,
            PasswordCreated = this.modelFactory.NextDateTime(),
        };
        await this.DatabaseFixture.InsertEntities(userBefore);

        var userAfterConcurrentModification = userBefore with { };

        this.SetPasswordAuthenticationRateLimiterResult(userBefore.Email, true);
        this.SetupVerifyHashedPassword(
            userBefore,
            hashedPassword,
            password,
            PasswordVerificationResult.SuccessRehashNeeded);
        this.passwordHasherMock
            .Setup(x => x.HashPassword(userBefore, password))
            .Callback(() =>
            {
                using var dbContext = this.DatabaseFixture.CreateDbContext();
                dbContext.Users.Attach(userAfterConcurrentModification);
                userAfterConcurrentModification.Name = this.modelFactory.NextString("name");
                userAfterConcurrentModification.Revision++;
                dbContext.SaveChanges();
            });

        var result = await this.passwordAuthenticationService.Authenticate(
            userBefore.Email, password, ipAddress);

        using var dbContext = this.DatabaseFixture.CreateDbContext();

        // Inserts user audit entry
        await this.AssertSingleUserAuditEntry(
            dbContext,
            UserAuditOperation.AuthenticatePassword,
            userBefore.Id,
            userBefore.Id,
            ipAddress);

        // Logs successfully authenticated message
        LogAssert.SingleEntry(this.logger, 1)
            .HasLevel(LogLevel.Information)
            .HasMessage($"User {userBefore.Id} ({userBefore.Email}) successfully authenticated");

        // Does not update user in database
        Assert.Equal(
            userAfterConcurrentModification,
            await dbContext.Users.FindAsync(
                [userBefore.Id], TestContext.Current.CancellationToken));

        // Logs upgraded password hash not persisted message
        LogAssert.SingleEntry(this.logger, 16)
            .HasLevel(LogLevel.Information)
            .HasMessage(
                $"Upgraded password hash not persisted for user {userBefore.Id} ({userBefore.Email}); concurrent changed detected");

        // Returns unmodified user
        Assert.Equal(userBefore, result.User);
    }

    private void SetPasswordAuthenticationRateLimiterResult(string email, bool isAllowed) =>
        this.passwordAuthenticationRateLimiterMock
            .Setup(x => x.IsAllowed(email))
            .ReturnsAsync(isAllowed);

    #endregion

    #region CanResetPassword

    [Fact]
    public async Task CanResetPassword_TokenExpired()
    {
        var user = this.modelFactory.BuildUser();
        var token = this.modelFactory.BuildPasswordResetToken(user) with
        {
            Created = this.timeProvider.GetUtcDateTimeNow().Subtract(new TimeSpan(24, 0, 1)),
        };
        await this.DatabaseFixture.InsertEntities(user, token);

        var maskedToken = this.SetupMaskToken(token.Token);
        var ipAddress = this.modelFactory.NextIpAddress();

        var result = await this.passwordAuthenticationService.CanResetPassword(
            token.Token, ipAddress);

        using var dbContext = this.DatabaseFixture.CreateDbContext();

        // Logs invalid token message
        LogAssert.SingleEntry(this.logger)
            .HasId(14)
            .HasLevel(LogLevel.Information)
            .HasMessage(
                $"Cannot use token '{maskedToken}' to reset password; token is invalid");

        // Returns 'invalid token' failure
        Assert.False(result.IsSuccess);
        Assert.Equal(PasswordResetFailure.InvalidToken, result.Failure);
    }

    [Fact]
    public async Task CanResetPassword_TokenNonExistent()
    {
        var user = this.modelFactory.BuildUser();
        var otherToken = this.modelFactory.BuildPasswordResetToken(user) with
        {
            Created = this.timeProvider.GetUtcDateTimeNow(),
        };
        await this.DatabaseFixture.InsertEntities(user, otherToken);

        var token = this.modelFactory.NextString("token");
        var maskedToken = this.SetupMaskToken(token);
        var ipAddress = this.modelFactory.NextIpAddress();

        var result = await this.passwordAuthenticationService.CanResetPassword(token, ipAddress);

        using var dbContext = this.DatabaseFixture.CreateDbContext();

        // Logs invalid token message
        LogAssert.SingleEntry(this.logger)
            .HasId(14)
            .HasLevel(LogLevel.Information)
            .HasMessage(
                $"Cannot use token '{maskedToken}' to reset password; token is invalid");

        // Returns 'invalid token' failure
        Assert.False(result.IsSuccess);
        Assert.Equal(PasswordResetFailure.InvalidToken, result.Failure);
    }

    [Fact]
    public async Task CanResetPassword_UserDeactivated()
    {
        var user = this.modelFactory.BuildUser(deactivated: true);
        var token = this.modelFactory.BuildPasswordResetToken(user) with
        {
            Created = this.timeProvider.GetUtcDateTimeNow(),
        };
        await this.DatabaseFixture.InsertEntities(user, token);

        var maskedToken = this.SetupMaskToken(token.Token);
        var ipAddress = this.modelFactory.NextIpAddress();

        var result = await this.passwordAuthenticationService.CanResetPassword(
            token.Token, ipAddress);

        using var dbContext = this.DatabaseFixture.CreateDbContext();

        // Inserts user audit entry
        await this.AssertSingleUserAuditEntry(
            dbContext,
            UserAuditOperation.ResetPassword,
            user.Id,
            user.Id,
            ipAddress,
            UserAuditFailure.UserDeactivated);

        // Logs user deactivated message
        LogAssert.SingleEntry(this.logger)
            .HasId(18)
            .HasLevel(LogLevel.Information)
            .HasMessage(
                $"Cannot use token '{maskedToken}' to reset password; user {user.Id} ({user.Email}) is deactivated");

        // Returns 'user deactivated' failure
        Assert.False(result.IsSuccess);
        Assert.Equal(PasswordResetFailure.UserDeactivated, result.Failure);
    }

    [Fact]
    public async Task CanResetPassword_Success()
    {
        var user = this.modelFactory.BuildUser();
        var token = this.modelFactory.BuildPasswordResetToken(user) with
        {
            Created = this.timeProvider.GetUtcDateTimeNow().Subtract(new TimeSpan(23, 59, 59)),
        };
        await this.DatabaseFixture.InsertEntities(user, token);

        var maskedToken = this.SetupMaskToken(token.Token);

        var result = await this.passwordAuthenticationService.CanResetPassword(
            token.Token, this.modelFactory.NextIpAddress());

        // Logs valid token message
        LogAssert.SingleEntry(this.logger)
            .HasId(15)
            .HasLevel(LogLevel.Debug)
            .HasMessage(
                $"Can use token '{maskedToken}' to reset password for user {user.Id} ({user.Email})");

        // Returns success result with affected user
        Assert.True(result.IsSuccess);
        Assert.Equal(user, result.User);
    }

    #endregion

    #region ChangePassword

    [Fact]
    public async Task ChangePassword_UserHasNoPassword()
    {
        var currentPassword = this.modelFactory.NextString("current-password");
        var newPassword = this.modelFactory.NextString("new-password");
        var ipAddress = this.modelFactory.NextIpAddress();

        var user = this.modelFactory.BuildUser() with { HashedPassword = null };
        await this.DatabaseFixture.InsertEntities(user);

        // Throws exception
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => this.passwordAuthenticationService.ChangePassword(
                user.Id, currentPassword, newPassword, ipAddress));

        using var dbContext = this.DatabaseFixture.CreateDbContext();

        // Inserts user audit entry
        await this.AssertSingleUserAuditEntry(
            dbContext,
            UserAuditOperation.ChangePassword,
            user.Id,
            user.Id,
            ipAddress,
            UserAuditFailure.NoPasswordSet);

        // Logs no password set message
        LogAssert.SingleEntry(this.logger)
            .HasId(20)
            .HasLevel(LogLevel.Information)
            .HasMessage(
                $"Password change denied; no password set for user {user.Id} ({user.Email})");
    }

    [Fact]
    public async Task ChangePassword_IncorrectCurrentPassword()
    {
        var currentPassword = this.modelFactory.NextString("current-password");
        var currentPasswordHash = this.modelFactory.NextString("hashed-current-password");
        var newPassword = this.modelFactory.NextString("new-password");
        var ipAddress = this.modelFactory.NextIpAddress();

        var user = this.modelFactory.BuildUser() with { HashedPassword = currentPasswordHash };
        await this.DatabaseFixture.InsertEntities(user);

        this.SetupVerifyHashedPassword(
            user, currentPasswordHash, currentPassword, PasswordVerificationResult.Failed);

        var result = await this.passwordAuthenticationService.ChangePassword(
            user.Id, currentPassword, newPassword, ipAddress);

        using var dbContext = this.DatabaseFixture.CreateDbContext();

        // Does not attempt to hash new password
        this.passwordHasherMock.Verify(x => x.HashPassword(user, newPassword), Times.Never);

        // Inserts user audit entry
        await this.AssertSingleUserAuditEntry(
            dbContext,
            UserAuditOperation.ChangePassword,
            user.Id,
            user.Id,
            ipAddress,
            UserAuditFailure.IncorrectPassword);

        // Logs password incorrect message
        LogAssert.SingleEntry(this.logger)
            .HasId(7)
            .HasLevel(LogLevel.Information)
            .HasMessage(
                $"Password change denied for user {user.Id} ({user.Email}); current password is incorrect");

        // Returns false
        Assert.False(result);
    }

    [Theory]
    [InlineData(PasswordVerificationResult.Success)]
    [InlineData(PasswordVerificationResult.SuccessRehashNeeded)]
    public async Task ChangePassword_Success(PasswordVerificationResult passwordVerificationResult)
    {
        var currentPassword = this.modelFactory.NextString("current-password");
        var currentPasswordHash = this.modelFactory.NextString("hashed-current-password");
        var newPassword = this.modelFactory.NextString("new-password");
        var ipAddress = this.modelFactory.NextIpAddress();

        var userBefore = this.modelFactory.BuildUser() with
        {
            HashedPassword = currentPasswordHash,
        };
        var otherUser = this.modelFactory.BuildUser();
        var passwordResetTokenForUser = this.modelFactory.BuildPasswordResetToken(userBefore);
        var passwordResetTokenForOtherUser = this.modelFactory.BuildPasswordResetToken(otherUser);
        await this.DatabaseFixture.InsertEntities(
            userBefore, otherUser, passwordResetTokenForUser, passwordResetTokenForOtherUser);

        this.SetupVerifyHashedPassword(
            userBefore, currentPasswordHash, currentPassword, passwordVerificationResult);
        var newPasswordHash = this.SetupHashPassword(userBefore, newPassword);
        var newSecurityStamp = this.SetupGenerateSecurityStamp();

        var result = await this.passwordAuthenticationService.ChangePassword(
            userBefore.Id, currentPassword, newPassword, ipAddress);

        using var dbContext = this.DatabaseFixture.CreateDbContext();

        // Updates user in database
        var expectedUserAfter = userBefore with
        {
            HashedPassword = newPasswordHash,
            PasswordCreated = this.timeProvider.GetUtcDateTimeNow(),
            SecurityStamp = newSecurityStamp,
            Modified = this.timeProvider.GetUtcDateTimeNow(),
            Revision = userBefore.Revision + 1,
        };
        var actualUserAfter = await dbContext.Users.FindAsync(
                [userBefore.Id], TestContext.Current.CancellationToken);
        Assert.Equal(expectedUserAfter, actualUserAfter);

        // Deletes all password reset tokens for the user
        Assert.Equal(
            passwordResetTokenForOtherUser.Token,
            await dbContext
                .PasswordResetTokens
                .Select(t => t.Token)
                .SingleAsync(TestContext.Current.CancellationToken));

        // Inserts user audit entry
        await this.AssertSingleUserAuditEntry(
            dbContext,
            UserAuditOperation.ChangePassword,
            userBefore.Id,
            userBefore.Id,
            ipAddress);

        // Logs password changed message
        LogAssert.SingleEntry(this.logger)
            .HasId(6)
            .HasLevel(LogLevel.Information)
            .HasMessage(
                $"Password successfully changed for user {userBefore.Id} ({userBefore.Email})");

        // Sends password change notification
        this.authenticationMailerMock.Verify(
            x => x.SendPasswordChangeNotification(userBefore.Email));

        // Returns true
        Assert.True(result);
    }

    #endregion

    #region ResetPassword

    [Fact]
    public async Task ResetPassword_TokenExpired()
    {
        var user = this.modelFactory.BuildUser();
        var token = this.modelFactory.BuildPasswordResetToken(user) with
        {
            Created = this.timeProvider.GetUtcDateTimeNow().Subtract(new TimeSpan(24, 0, 1)),
        };
        await this.DatabaseFixture.InsertEntities(user, token);

        var maskedToken = this.SetupMaskToken(token.Token);
        var newPassword = this.modelFactory.NextString("new-password");
        var ipAddress = this.modelFactory.NextIpAddress();

        var result = await this.passwordAuthenticationService.ResetPassword(
            token.Token, newPassword, ipAddress);

        using var dbContext = this.DatabaseFixture.CreateDbContext();

        // Logs invalid token message
        LogAssert.SingleEntry(this.logger)
            .HasId(10)
            .HasLevel(LogLevel.Information)
            .HasMessage(
                $"Unable to reset password; password reset token {maskedToken} is invalid");

        // Returns 'invalid token' failure
        Assert.False(result.IsSuccess);
        Assert.Equal(PasswordResetFailure.InvalidToken, result.Failure);
    }

    [Fact]
    public async Task ResetPassword_TokenNonExistent()
    {
        var user = this.modelFactory.BuildUser();
        var otherToken = this.modelFactory.BuildPasswordResetToken(user) with
        {
            Created = this.timeProvider.GetUtcDateTimeNow(),
        };
        await this.DatabaseFixture.InsertEntities(user, otherToken);

        var token = this.modelFactory.NextString("token");
        var maskedToken = this.SetupMaskToken(token);
        var newPassword = this.modelFactory.NextString("new-password");
        var ipAddress = this.modelFactory.NextIpAddress();

        var result = await this.passwordAuthenticationService.ResetPassword(
            token, newPassword, ipAddress);

        using var dbContext = this.DatabaseFixture.CreateDbContext();

        // Logs invalid token message
        LogAssert.SingleEntry(this.logger)
            .HasId(10)
            .HasLevel(LogLevel.Information)
            .HasMessage(
                $"Unable to reset password; password reset token {maskedToken} is invalid");

        // Returns 'invalid token' failure
        Assert.False(result.IsSuccess);
        Assert.Equal(PasswordResetFailure.InvalidToken, result.Failure);
    }

    [Fact]
    public async Task ResetPassword_UserDeactivated()
    {
        var user = this.modelFactory.BuildUser(deactivated: true);
        var token = this.modelFactory.BuildPasswordResetToken(user) with
        {
            Created = this.timeProvider.GetUtcDateTimeNow(),
        };
        await this.DatabaseFixture.InsertEntities(user, token);

        var maskedToken = this.SetupMaskToken(token.Token);
        var newPassword = this.modelFactory.NextString("new-password");
        var ipAddress = this.modelFactory.NextIpAddress();

        var result = await this.passwordAuthenticationService.ResetPassword(
            token.Token, newPassword, ipAddress);

        using var dbContext = this.DatabaseFixture.CreateDbContext();

        // Inserts user audit entry
        await this.AssertSingleUserAuditEntry(
            dbContext,
            UserAuditOperation.ResetPassword,
            user.Id,
            user.Id,
            ipAddress,
            UserAuditFailure.UserDeactivated);

        // Logs user deactivated message
        LogAssert.SingleEntry(this.logger)
            .HasId(19)
            .HasLevel(LogLevel.Information)
            .HasMessage(
                $"Unable to reset password using token {maskedToken}; user {user.Id} ({user.Email}) is deactivated");

        // Returns 'user deactivated' failure
        Assert.False(result.IsSuccess);
        Assert.Equal(PasswordResetFailure.UserDeactivated, result.Failure);
    }

    [Fact]
    public async Task ResetPassword_Success()
    {
        var userBefore = this.modelFactory.BuildUser();
        var token = this.modelFactory.BuildPasswordResetToken(userBefore) with
        {
            Created = this.timeProvider.GetUtcDateTimeNow().Subtract(new TimeSpan(23, 59, 59))
        };
        var otherUser = this.modelFactory.BuildUser();
        var tokenForOtherUser = this.modelFactory.BuildPasswordResetToken(otherUser);
        await this.DatabaseFixture.InsertEntities(
            userBefore, otherUser, token, tokenForOtherUser);

        var maskedToken = this.SetupMaskToken(token.Token);
        var newPassword = this.modelFactory.NextString("new-password");
        var newPasswordHash = this.SetupHashPassword(userBefore, newPassword);
        var newSecurityStamp = this.SetupGenerateSecurityStamp();
        var ipAddress = this.modelFactory.NextIpAddress();

        var result = await this.passwordAuthenticationService.ResetPassword(
            token.Token, newPassword, ipAddress);

        using var dbContext = this.DatabaseFixture.CreateDbContext();

        // Updates user in database
        var expectedUserAfter = userBefore with
        {
            HashedPassword = newPasswordHash,
            PasswordCreated = this.timeProvider.GetUtcDateTimeNow(),
            SecurityStamp = newSecurityStamp,
            Modified = this.timeProvider.GetUtcDateTimeNow(),
            Revision = userBefore.Revision + 1,
        };
        var actualUserAfter = await dbContext.Users.FindAsync(
            [userBefore.Id], TestContext.Current.CancellationToken);
        Assert.Equal(expectedUserAfter, actualUserAfter);

        // Deletes all password reset tokens for the user
        Assert.Equal(
            tokenForOtherUser.Token,
            await dbContext
                .PasswordResetTokens
                .Select(t => t.Token)
                .SingleAsync(TestContext.Current.CancellationToken));

        // Inserts user audit entry
        await this.AssertSingleUserAuditEntry(
            dbContext,
            UserAuditOperation.ResetPassword,
            userBefore.Id,
            userBefore.Id,
            ipAddress);

        // Logs password reset message
        LogAssert.SingleEntry(this.logger)
            .HasId(9)
            .HasLevel(LogLevel.Information)
            .HasMessage($"Password reset for user {userBefore.Id} using token {maskedToken}");

        // Sends password change notification
        this.authenticationMailerMock.Verify(
            x => x.SendPasswordChangeNotification(userBefore.Email));

        // Resets the rate limit counters
        this.passwordAuthenticationRateLimiterMock.Verify(x => x.Reset(userBefore.Email));

        // Returns success result with updated user
        Assert.True(result.IsSuccess);
        Assert.Equal(expectedUserAfter, result.User);
    }

    #endregion

    #region SendPasswordResetLink

    [Fact]
    public async Task SendPasswordResetLink_RateLimitExceeded()
    {
        var email = this.modelFactory.NextEmail();
        var ipAddress = this.modelFactory.NextIpAddress();

        this.SetPasswordResetRateLimiterResult(email, false);

        var result = await this.passwordAuthenticationService.SendPasswordResetLink(
            email, ipAddress, Mock.Of<IUrlHelper>());

        using var dbContext = this.DatabaseFixture.CreateDbContext();

        // Logs rate limit exceeded message
        LogAssert.SingleEntry(this.logger)
            .HasId(11)
            .HasLevel(LogLevel.Information)
            .HasMessage($"Unable to send password reset link to {email}; rate limit exceeded");

        // Returns false
        Assert.False(result);
    }

    [Fact]
    public async Task SendPasswordResetLink_EmailUnrecognized()
    {
        var email = this.modelFactory.NextEmail();
        var ipAddress = this.modelFactory.NextIpAddress();

        this.SetPasswordResetRateLimiterResult(email, true);

        await this.DatabaseFixture.InsertEntities(this.modelFactory.BuildUser());

        var result = await this.passwordAuthenticationService.SendPasswordResetLink(
            email, ipAddress, Mock.Of<IUrlHelper>());

        using var dbContext = this.DatabaseFixture.CreateDbContext();

        // Logs unrecognized email message
        LogAssert.SingleEntry(this.logger)
            .HasId(12)
            .HasLevel(LogLevel.Information)
            .HasMessage($"Unable to send password reset link to {email}; no matching user");

        // Returns true
        Assert.True(result);
    }

    [Fact]
    public async Task SendPasswordResetLink_Success()
    {
        var ipAddress = this.modelFactory.NextIpAddress();
        var token = this.modelFactory.NextString("token");
        var link = this.modelFactory.NextString("https://example.com/reset-password/token");
        var urlHelperMock = new Mock<IUrlHelper>();

        var user = this.modelFactory.BuildUser();
        await this.DatabaseFixture.InsertEntities(user);

        this.SetPasswordResetRateLimiterResult(user.Email, true);

        this.randomTokenGeneratorMock.Setup(x => x.Generate(12)).Returns(token);
        urlHelperMock
            .Setup(
                x => x.Link(
                    "ResetPassword",
                    It.Is<object>(o => token.Equals(new RouteValueDictionary(o)["token"]))))
            .Returns(link);

        var result = await this.passwordAuthenticationService.SendPasswordResetLink(
            user.Email, ipAddress, urlHelperMock.Object);

        using var dbContext = this.DatabaseFixture.CreateDbContext();

        // Inserts password reset token
        Assert.Equal(
            new PasswordResetToken
            {
                Token = token,
                UserId = user.Id,
                Created = this.timeProvider.GetUtcDateTimeNow(),
            },
            dbContext.PasswordResetTokens.Single());

        // Sends link to user
        this.authenticationMailerMock.Verify(x => x.SendPasswordResetLink(user.Email, link));

        // Inserts user audit entry
        await this.AssertSingleUserAuditEntry(
            dbContext,
            UserAuditOperation.CreatePasswordResetToken,
            user.Id,
            user.Id,
            ipAddress);

        // Logs password reset link sent message
        LogAssert.SingleEntry(this.logger)
            .HasId(13)
            .HasLevel(LogLevel.Information)
            .HasMessage($"Password reset link sent to user {user.Id} ({user.Email})");

        // Returns true
        Assert.True(result);
    }

    private void SetPasswordResetRateLimiterResult(string email, bool isAllowed) =>
        this.passwordResetRateLimiterMock.Setup(x => x.IsAllowed(email)).ReturnsAsync(isAllowed);

    #endregion

    private string SetupGenerateSecurityStamp()
    {
        var securityStamp =
            this.modelFactory.NextInt().ToString("X8", CultureInfo.InvariantCulture);
        this.randomTokenGeneratorMock.Setup(x => x.Generate(2)).Returns(securityStamp);
        return securityStamp;
    }

    private string SetupHashPassword(User user, string newPassword)
    {
        var passwordHash = this.modelFactory.NextString("password-hash");
        this.passwordHasherMock.Setup(x => x.HashPassword(user, newPassword)).Returns(passwordHash);
        return passwordHash;
    }

    private string SetupMaskEmail(string email)
    {
        var maskedEmail = this.modelFactory.NextString("masked-email");
        this.parameterMaskingServiceMock.Setup(x => x.MaskEmail(email)).Returns(maskedEmail);
        return maskedEmail;
    }

    private string SetupMaskToken(string token)
    {
        var maskedToken = this.modelFactory.NextString("masked-token");
        this.parameterMaskingServiceMock.Setup(x => x.MaskToken(token)).Returns(maskedToken);
        return maskedToken;
    }

    private void SetupVerifyHashedPassword(
        User user,
        string hashedPassword,
        string suppliedPassword,
        PasswordVerificationResult result) =>
        this.passwordHasherMock
            .Setup(x => x.VerifyHashedPassword(user, hashedPassword, suppliedPassword))
            .Returns(result);

    private async Task AssertSingleUserAuditEntry(
        AppDbContext dbContext,
        UserAuditOperation operation,
        long targetId,
        long actorId,
        IPAddress ipAddress,
        UserAuditFailure? failure = null)
    {
        var actual = await dbContext.UserAuditEntries.SingleAsync(
            TestContext.Current.CancellationToken);
        var expected = new UserAuditEntry
        {
            Id = actual.Id,
            Time = this.timeProvider.GetUtcDateTimeNow(),
            Operation = operation,
            TargetId = targetId,
            ActorId = actorId,
            IpAddress = ipAddress,
            Failure = failure,
        };
        Assert.Equal(expected, actual);
    }
}
