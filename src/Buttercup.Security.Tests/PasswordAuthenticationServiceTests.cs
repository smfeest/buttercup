using System.Globalization;
using System.Net;
using Buttercup.EntityModel;
using Buttercup.TestUtils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Xunit;

namespace Buttercup.Security;

[Collection(nameof(DatabaseCollection))]
public sealed class PasswordAuthenticationServiceTests : DatabaseTests<DatabaseCollection>
{
    private readonly ModelFactory modelFactory = new();

    private readonly Mock<IAuthenticationMailer> authenticationMailerMock = new();
    private readonly ListLogger<PasswordAuthenticationService> logger = new();
    private readonly Mock<IPasswordHasher<User>> passwordHasherMock = new();
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
            this.passwordHasherMock.Object,
            this.randomTokenGeneratorMock.Object,
            this.timeProvider);
    }

    #region Authenticate

    [Fact]
    public async Task Authenticate_EmailUnrecognized()
    {
        var args = this.BuildAuthenticateArgs();

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            dbContext.Users.Add(this.modelFactory.BuildUser());
            await dbContext.SaveChangesAsync();
        }

        var result = await this.passwordAuthenticationService.Authenticate(
            args.Email, args.Password, args.IpAddress);

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            // Inserts security event
            Assert.True(
                await this.SecurityEventExists(
                    dbContext, "authentication_failure:unrecognized_email", args.IpAddress));

            // Logs unrecognized email message
            LogAssert.HasEntry(
                this.logger,
                LogLevel.Information,
                203,
                $"Authentication failed; no user with email {args.Email}");

            // Returns null
            Assert.Null(result);
        }
    }

    [Fact]
    public async Task Authenticate_UserHasNoPassword()
    {
        var args = this.BuildAuthenticateArgs();
        var user = this.modelFactory.BuildUser() with { Email = args.Email, HashedPassword = null };

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
        }

        var result = await this.passwordAuthenticationService.Authenticate(
            args.Email, args.Password, args.IpAddress);

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            // Inserts security event
            Assert.True(
                await this.SecurityEventExists(
                    dbContext, "authentication_failure:no_password_set", args.IpAddress, user.Id));

            // Logs no password set message
            LogAssert.HasEntry(
                this.logger,
                LogLevel.Information,
                201,
                $"Authentication failed; no password set for user {user.Id} ({user.Email})");

            // Returns null
            Assert.Null(result);
        }
    }

    [Fact]
    public async Task Authenticate_IncorrectPassword()
    {
        var args = this.BuildAuthenticateArgs();
        var hashedPassword = this.modelFactory.NextString("hashed-password");
        var user = this.modelFactory.BuildUser() with
        {
            Email = args.Email,
            HashedPassword = hashedPassword,
        };

        this.SetupVerifyHashedPassword(
            user, hashedPassword, args.Password, PasswordVerificationResult.Failed);

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
        }

        var result = await this.passwordAuthenticationService.Authenticate(
            args.Email, args.Password, args.IpAddress);

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            // Inserts security event
            Assert.True(
                await this.SecurityEventExists(
                    dbContext, "authentication_failure:incorrect_password", args.IpAddress, user.Id));

            // Logs incorrect password message
            LogAssert.HasEntry(
                this.logger,
                LogLevel.Information,
                200,
                $"Authentication failed; incorrect password for user {user.Id} ({user.Email})");

            // Returns null
            Assert.Null(result);
        }
    }

    [Fact]
    public async Task Authenticate_SuccessRehashNotNeeded()
    {
        var args = this.BuildAuthenticateArgs();
        var hashedPassword = this.modelFactory.NextString("hashed-password");
        var user = this.modelFactory.BuildUser() with
        {
            Email = args.Email,
            HashedPassword = hashedPassword,
        };

        this.SetupVerifyHashedPassword(
            user, hashedPassword, args.Password, PasswordVerificationResult.Success);

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
        }

        var result = await this.passwordAuthenticationService.Authenticate(
            args.Email, args.Password, args.IpAddress);

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            // Inserts security event
            Assert.True(
                await this.SecurityEventExists(
                    dbContext, "authentication_success", args.IpAddress, user.Id));

            // Logs successfully authenticated message
            LogAssert.HasEntry(
                this.logger,
                LogLevel.Information,
                202,
                $"User {user.Id} ({user.Email}) successfully authenticated");

            // Does not rehash password
            this.passwordHasherMock.Verify(x => x.HashPassword(user, args.Password), Times.Never);

            // Returns user
            Assert.Equal(user, result);
        }
    }

    [Fact]
    public async Task Authenticate_SuccessRehashNeeded()
    {
        var args = this.BuildAuthenticateArgs();
        var hashedPassword = this.modelFactory.NextString("hashed-password");
        var userBefore = this.modelFactory.BuildUser() with
        {
            Email = args.Email,
            HashedPassword = hashedPassword,
            PasswordCreated = this.modelFactory.NextDateTime(),
        };

        this.SetupVerifyHashedPassword(
            userBefore,
            hashedPassword,
            args.Password,
            PasswordVerificationResult.SuccessRehashNeeded);
        var rehashedPassword = this.SetupHashPassword(userBefore, args.Password);

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            dbContext.Users.Add(userBefore);
            await dbContext.SaveChangesAsync();
        }

        var result = await this.passwordAuthenticationService.Authenticate(
            args.Email, args.Password, args.IpAddress);

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            // Inserts security event
            Assert.True(
                await this.SecurityEventExists(
                    dbContext, "authentication_success", args.IpAddress, userBefore.Id));

            // Logs successfully authenticated message
            LogAssert.HasEntry(
                this.logger,
                LogLevel.Information,
                202,
                $"User {userBefore.Id} ({userBefore.Email}) successfully authenticated");

            var expectedUserAfter = userBefore with
            {
                HashedPassword = rehashedPassword,
                Modified = this.timeProvider.GetUtcDateTimeNow(),
                Revision = userBefore.Revision + 1,
            };

            // Updates user in database
            Assert.Equal(expectedUserAfter, await dbContext.Users.FindAsync(userBefore.Id));

            // Logs password hash upgraded message
            LogAssert.HasEntry(
                this.logger,
                LogLevel.Information,
                217,
                $"Password hash upgraded for user {userBefore.Id} ({userBefore.Email})");

            // Returns updated user
            Assert.Equal(expectedUserAfter, result);
        }
    }

    [Fact]
    public async Task Authenticate_PasswordRehashNotPersisted()
    {
        var args = this.BuildAuthenticateArgs();
        var hashedPassword = this.modelFactory.NextString("hashed-password");
        var userBefore = this.modelFactory.BuildUser() with
        {
            Email = args.Email,
            HashedPassword = hashedPassword,
            PasswordCreated = this.modelFactory.NextDateTime(),
        };
        var userAfterConcurrentModification = userBefore with { };

        this.SetupVerifyHashedPassword(
            userBefore,
            hashedPassword,
            args.Password,
            PasswordVerificationResult.SuccessRehashNeeded);
        this.passwordHasherMock
            .Setup(x => x.HashPassword(userBefore, args.Password))
            .Callback(() =>
            {
                using var dbContext = this.DatabaseFixture.CreateDbContext();
                dbContext.Users.Attach(userAfterConcurrentModification);
                userAfterConcurrentModification.Name = this.modelFactory.NextString("name");
                userAfterConcurrentModification.Revision++;
                dbContext.SaveChanges();
            });

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            dbContext.Users.Add(userBefore);
            await dbContext.SaveChangesAsync();
        }

        var result = await this.passwordAuthenticationService.Authenticate(
            args.Email, args.Password, args.IpAddress);

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            // Inserts security event
            Assert.True(
                await this.SecurityEventExists(
                    dbContext, "authentication_success", args.IpAddress, userBefore.Id));

            // Logs successfully authenticated message
            LogAssert.HasEntry(
                this.logger,
                LogLevel.Information,
                202,
                $"User {userBefore.Id} ({userBefore.Email}) successfully authenticated");

            // Does not update user in database
            Assert.Equal(
                userAfterConcurrentModification, await dbContext.Users.FindAsync(userBefore.Id));

            // Logs upgraded password hash not persisted message
            LogAssert.HasEntry(
                this.logger,
                LogLevel.Information,
                218,
                $"Upgraded password hash not persisted for user {userBefore.Id} ({userBefore.Email}); concurrent changed detected");

            // Returns unmodified user
            Assert.Equal(userBefore, result);
        }
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
        var user = this.modelFactory.BuildUser() with { Id = args.UserId, HashedPassword = null };

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
        }

        // Throws exception
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => this.passwordAuthenticationService.ChangePassword(
                args.UserId, args.CurrentPassword, args.NewPassword, args.IpAddress));

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            // Inserts security event
            Assert.True(
                await this.SecurityEventExists(
                    dbContext, "password_change_failure:no_password_set", args.IpAddress, user.Id));
        }
    }

    [Fact]
    public async Task ChangePassword_IncorrectCurrentPassword()
    {
        var args = this.BuildChangePasswordArgs();
        var currentPasswordHash = this.modelFactory.NextString("hashed-current-password");
        var user = this.modelFactory.BuildUser() with
        {
            Id = args.UserId,
            HashedPassword = currentPasswordHash,
        };

        this.SetupVerifyHashedPassword(
            user, currentPasswordHash, args.CurrentPassword, PasswordVerificationResult.Failed);

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
        }

        var result = await this.passwordAuthenticationService.ChangePassword(
            args.UserId, args.CurrentPassword, args.NewPassword, args.IpAddress);

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            // Does not attempt to hash new password
            this.passwordHasherMock.Verify(
                x => x.HashPassword(user, args.NewPassword), Times.Never);

            // Inserts security event
            Assert.True(
                await this.SecurityEventExists(
                    dbContext,
                    "password_change_failure:incorrect_password",
                    args.IpAddress,
                    user.Id));

            // Logs password incorrect message
            LogAssert.HasEntry(
                this.logger,
                LogLevel.Information,
                204,
                $"Password change denied for user {user.Id} ({user.Email}); current password is incorrect");

            // Returns false
            Assert.False(result);
        }
    }

    [Theory]
    [InlineData(PasswordVerificationResult.Success)]
    [InlineData(PasswordVerificationResult.SuccessRehashNeeded)]
    public async Task ChangePassword_Success(PasswordVerificationResult passwordVerificationResult)
    {
        var args = this.BuildChangePasswordArgs();
        var currentPasswordHash = this.modelFactory.NextString("hashed-current-password");
        var userBefore = this.modelFactory.BuildUser() with
        {
            Id = args.UserId,
            HashedPassword = currentPasswordHash,
        };
        var otherUser = this.modelFactory.BuildUser();
        var passwordResetTokenForUser =
            this.modelFactory.BuildPasswordResetToken() with { UserId = userBefore.Id };
        var passwordResetTokenForOtherUser =
            this.modelFactory.BuildPasswordResetToken() with { UserId = otherUser.Id };

        this.SetupVerifyHashedPassword(
            userBefore, currentPasswordHash, args.CurrentPassword, passwordVerificationResult);
        var newPasswordHash = this.SetupHashPassword(userBefore, args.NewPassword);
        var newSecurityStamp = this.SetupGenerateSecurityStamp();

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            dbContext.Users.AddRange(userBefore, otherUser);
            dbContext.PasswordResetTokens.AddRange(
                passwordResetTokenForUser, passwordResetTokenForOtherUser);
            await dbContext.SaveChangesAsync();
        }

        var result = await this.passwordAuthenticationService.ChangePassword(
            args.UserId, args.CurrentPassword, args.NewPassword, args.IpAddress);

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            var expectedUserAfter = userBefore with
            {
                HashedPassword = newPasswordHash,
                PasswordCreated = this.timeProvider.GetUtcDateTimeNow(),
                SecurityStamp = newSecurityStamp,
                Modified = this.timeProvider.GetUtcDateTimeNow(),
                Revision = userBefore.Revision + 1,
            };

            // Updates user in database
            Assert.Equal(expectedUserAfter, await dbContext.Users.FindAsync(userBefore.Id));

            // Deletes all password reset tokens for the user
            Assert.Equal(
                passwordResetTokenForOtherUser.Token,
                await dbContext.PasswordResetTokens.Select(t => t.Token).SingleAsync());

            // Inserts security event
            Assert.True(
                await this.SecurityEventExists(
                    dbContext, "password_change_success", args.IpAddress, userBefore.Id));

            // Logs password changed message
            LogAssert.HasEntry(
                this.logger,
                LogLevel.Information,
                205,
                $"Password successfully changed for user {userBefore.Id} ({userBefore.Email})");

            // Sends password change notification
            this.authenticationMailerMock.Verify(
                x => x.SendPasswordChangeNotification(userBefore.Email));

            // Returns true
            Assert.True(result);
        }
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
        var user = this.modelFactory.BuildUser();

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            dbContext.Users.Add(user);
            dbContext.PasswordResetTokens.Add(
                new()
                {
                    UserId = user.Id,
                    Token = args.Token,
                    Created = this.timeProvider.GetUtcDateTimeNow().Subtract(
                        new TimeSpan(23, 59, 59))
                });
            await dbContext.SaveChangesAsync();
        }

        var result = await this.passwordAuthenticationService.PasswordResetTokenIsValid(
            args.Token, args.IpAddress);

        // Logs valid token message
        LogAssert.HasEntry(
            this.logger,
            LogLevel.Debug,
                207,
                $"Password reset token '{args.Token[..6]}…' is valid and belongs to user {user.Id}");

        // Returns true
        Assert.True(result);
    }

    [Fact]
    public async Task PasswordResetTokenIsValid_Expired()
    {
        var args = this.BuildPasswordResetTokenIsValidArgs();

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            var user = this.modelFactory.BuildUser();
            dbContext.Users.Add(user);
            dbContext.PasswordResetTokens.Add(
                new()
                {
                    UserId = user.Id,
                    Token = args.Token,
                    Created = this.timeProvider.GetUtcDateTimeNow().Subtract(new TimeSpan(24, 0, 1))
                });
            await dbContext.SaveChangesAsync();
        }

        var result = await this.passwordAuthenticationService.PasswordResetTokenIsValid(
            args.Token, args.IpAddress);

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            // Inserts security event
            Assert.True(
                await this.SecurityEventExists(
                    dbContext, "password_reset_failure:invalid_token", args.IpAddress));

            // Logs invalid token message
            LogAssert.HasEntry(
                this.logger,
                LogLevel.Debug,
                206,
                $"Password reset token '{args.Token[..6]}…' is no longer valid");

            // Returns false
            Assert.False(result);
        }
    }

    [Fact]
    public async Task PasswordResetTokenIsValid_NonExistent()
    {
        var args = this.BuildPasswordResetTokenIsValidArgs();

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            var user = this.modelFactory.BuildUser();
            dbContext.Users.Add(user);
            dbContext.PasswordResetTokens.Add(
                new()
                {
                    UserId = user.Id,
                    Token = this.modelFactory.NextString("token"),
                    Created = this.timeProvider.GetUtcDateTimeNow(),
                });
            await dbContext.SaveChangesAsync();
        }

        var result = await this.passwordAuthenticationService.PasswordResetTokenIsValid(
            args.Token, args.IpAddress);

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            // Inserts security event
            Assert.True(
                await this.SecurityEventExists(
                    dbContext, "password_reset_failure:invalid_token", args.IpAddress));

            // Logs invalid token message
            LogAssert.HasEntry(
                this.logger,
                LogLevel.Debug,
                206,
                $"Password reset token '{args.Token[..6]}…' is no longer valid");

            // Returns false
            Assert.False(result);
        }
    }

    private sealed record PasswordResetTokenIsValidArgs(string Token, IPAddress IpAddress);

    private PasswordResetTokenIsValidArgs BuildPasswordResetTokenIsValidArgs() => new(
        this.modelFactory.NextString("token"), new(this.modelFactory.NextInt()));

    #endregion

    #region ResetPassword

    [Fact]
    public async Task ResetPassword_ExpiredToken()
    {
        var args = this.BuildResetPasswordArgs();

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            var user = this.modelFactory.BuildUser();
            dbContext.Users.Add(user);
            dbContext.PasswordResetTokens.Add(
                new()
                {
                    UserId = user.Id,
                    Token = args.Token,
                    Created = this.timeProvider.GetUtcDateTimeNow().Subtract(new TimeSpan(24, 0, 1))
                });
            await dbContext.SaveChangesAsync();
        }

        // Throws exception
        await Assert.ThrowsAsync<InvalidTokenException>(
            () => this.passwordAuthenticationService.ResetPassword(
                args.Token, args.NewPassword, args.IpAddress));

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            // Inserts security event
            Assert.True(
                await this.SecurityEventExists(
                    dbContext, "password_reset_failure:invalid_token", args.IpAddress));

            // Logs invalid token message
            LogAssert.HasEntry(
                this.logger,
                LogLevel.Information,
                208,
                $"Unable to reset password; password reset token {args.Token[..6]}… is invalid");
        }
    }

    [Fact]
    public async Task ResetPassword_NonExistantToken()
    {
        var args = this.BuildResetPasswordArgs();

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            var user = this.modelFactory.BuildUser();
            dbContext.Users.Add(user);
            dbContext.PasswordResetTokens.Add(
                new()
                {
                    UserId = user.Id,
                    Token = this.modelFactory.NextString("token"),
                    Created = this.timeProvider.GetUtcDateTimeNow(),
                });
            await dbContext.SaveChangesAsync();
        }

        // Throws exception
        await Assert.ThrowsAsync<InvalidTokenException>(
            () => this.passwordAuthenticationService.ResetPassword(
                args.Token, args.NewPassword, args.IpAddress));

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            // Inserts security event
            Assert.True(
                await this.SecurityEventExists(
                    dbContext, "password_reset_failure:invalid_token", args.IpAddress));

            // Logs invalid token message
            LogAssert.HasEntry(
                this.logger,
                LogLevel.Information,
                208,
                $"Unable to reset password; password reset token {args.Token[..6]}… is invalid");
        }
    }

    [Fact]
    public async Task ResetPassword_Success()
    {
        var args = this.BuildResetPasswordArgs();
        var userBefore = this.modelFactory.BuildUser();
        var otherUser = this.modelFactory.BuildUser();
        var passwordResetTokenForUser = new PasswordResetToken()
        {
            Token = args.Token,
            UserId = userBefore.Id,
            Created = this.timeProvider.GetUtcDateTimeNow().Subtract(new TimeSpan(23, 59, 59))
        };
        var passwordResetTokenForOtherUser =
            this.modelFactory.BuildPasswordResetToken() with { UserId = otherUser.Id };
        var newPasswordHash = this.SetupHashPassword(userBefore, args.NewPassword);
        var newSecurityStamp = this.SetupGenerateSecurityStamp();

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            dbContext.Users.AddRange(userBefore, otherUser);
            dbContext.PasswordResetTokens.AddRange(
                passwordResetTokenForUser, passwordResetTokenForOtherUser);
            await dbContext.SaveChangesAsync();
        }

        var result = await this.passwordAuthenticationService.ResetPassword(
            args.Token, args.NewPassword, args.IpAddress);

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            var expectedUserAfter = userBefore with
            {
                HashedPassword = newPasswordHash,
                PasswordCreated = this.timeProvider.GetUtcDateTimeNow(),
                SecurityStamp = newSecurityStamp,
                Modified = this.timeProvider.GetUtcDateTimeNow(),
                Revision = userBefore.Revision + 1,
            };

            // Updates user in database
            Assert.Equal(expectedUserAfter, await dbContext.Users.FindAsync(userBefore.Id));

            // Deletes all password reset tokens for the user
            Assert.Equal(
                passwordResetTokenForOtherUser.Token,
                await dbContext.PasswordResetTokens.Select(t => t.Token).SingleAsync());

            // Inserts security event
            Assert.True(
                await this.SecurityEventExists(
                    dbContext, "password_reset_success", args.IpAddress, userBefore.Id));

            // Logs password reset message
            LogAssert.HasEntry(
                this.logger,
                LogLevel.Information,
                209,
                $"Password reset for user {userBefore.Id} using token {args.Token[..6]}…");

            // Sends password change notification
            this.authenticationMailerMock.Verify(
                x => x.SendPasswordChangeNotification(userBefore.Email));

            // Returns updated user
            Assert.Equal(expectedUserAfter, result);
        }
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

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            dbContext.Users.Add(this.modelFactory.BuildUser());
            await dbContext.SaveChangesAsync();
        }

        await this.passwordAuthenticationService.SendPasswordResetLink(
            args.Email, args.IpAddress, Mock.Of<IUrlHelper>());

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            // Inserts security event
            Assert.True(
                await this.SecurityEventExists(
                    dbContext, "password_reset_failure:unrecognized_email", args.IpAddress));

            // Logs unrecognized email message
            LogAssert.HasEntry(
                this.logger,
                LogLevel.Information,
                211,
                $"Unable to send password reset link; No user with email {args.Email}");
        }
    }

    [Fact]
    public async Task SendPasswordResetLink_Success()
    {
        var args = this.BuildSendPasswordResetLinkArgs();
        var user = this.modelFactory.BuildUser() with { Email = args.Email };
        var token = this.modelFactory.NextString("token");
        var link = this.modelFactory.NextString("https://example.com/reset-password/token");
        var urlHelperMock = new Mock<IUrlHelper>();

        this.randomTokenGeneratorMock.Setup(x => x.Generate(12)).Returns(token);
        urlHelperMock
            .Setup(
                x => x.Link(
                    "ResetPassword",
                    It.Is<object>(o => token.Equals(new RouteValueDictionary(o)["token"]))))
            .Returns(link);

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
        }

        await this.passwordAuthenticationService.SendPasswordResetLink(
            args.Email, args.IpAddress, urlHelperMock.Object);

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
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

            // Inserts security event
            Assert.True(
                await this.SecurityEventExists(
                    dbContext, "password_reset_link_sent", args.IpAddress, user.Id));

            // Logs password reset link sent message
            LogAssert.HasEntry(
                this.logger,
                LogLevel.Information,
                210,
                $"Password reset link sent to user {user.Id} ({user.Email})");
        }
    }

    private sealed record SendPasswordResetLinkArgs(string Email, IPAddress IpAddress);

    private SendPasswordResetLinkArgs BuildSendPasswordResetLinkArgs() =>
        new(this.modelFactory.NextEmail(), new(this.modelFactory.NextInt()));

    #endregion

    private async Task<bool> SecurityEventExists(
        AppDbContext dbContext, string eventName, IPAddress ipAddress, long? userId = null) =>
        await dbContext.SecurityEvents.AnyAsync(
            securityEvent =>
                securityEvent.Time == this.timeProvider.GetUtcDateTimeNow() &&
                securityEvent.Event == eventName &&
                securityEvent.IpAddress == ipAddress &&
                securityEvent.UserId == userId);

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

    private void SetupVerifyHashedPassword(
        User user,
        string hashedPassword,
        string suppliedPassword,
        PasswordVerificationResult result) =>
        this.passwordHasherMock
            .Setup(x => x.VerifyHashedPassword(user, hashedPassword, suppliedPassword))
            .Returns(result);
}
