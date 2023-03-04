using System.Security.Claims;
using Buttercup.DataAccess;
using Buttercup.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using MySqlConnector;

namespace Buttercup.Web.Authentication;

public class AuthenticationManager : IAuthenticationManager
{
    private readonly IAuthenticationEventDataProvider authenticationEventDataProvider;
    private readonly IAuthenticationMailer authenticationMailer;
    private readonly IAuthenticationService authenticationService;
    private readonly IClock clock;
    private readonly IMySqlConnectionSource mySqlConnectionSource;
    private readonly ILogger<AuthenticationManager> logger;
    private readonly IPasswordHasher<User> passwordHasher;
    private readonly IPasswordResetTokenDataProvider passwordResetTokenDataProvider;
    private readonly IRandomTokenGenerator randomTokenGenerator;
    private readonly IUrlHelperFactory urlHelperFactory;
    private readonly IUserDataProvider userDataProvider;
    private readonly IUserPrincipalFactory userPrincipalFactory;

    public AuthenticationManager(
        IAuthenticationEventDataProvider authenticationEventDataProvider,
        IAuthenticationMailer authenticationMailer,
        IAuthenticationService authenticationService,
        IClock clock,
        IMySqlConnectionSource mySqlConnectionSource,
        ILogger<AuthenticationManager> logger,
        IPasswordHasher<User> passwordHasher,
        IPasswordResetTokenDataProvider passwordResetTokenDataProvider,
        IRandomTokenGenerator randomTokenGenerator,
        IUrlHelperFactory urlHelperFactory,
        IUserDataProvider userDataProvider,
        IUserPrincipalFactory userPrincipalFactory)
    {
        this.authenticationEventDataProvider = authenticationEventDataProvider;
        this.authenticationMailer = authenticationMailer;
        this.authenticationService = authenticationService;
        this.clock = clock;
        this.mySqlConnectionSource = mySqlConnectionSource;
        this.logger = logger;
        this.passwordHasher = passwordHasher;
        this.passwordResetTokenDataProvider = passwordResetTokenDataProvider;
        this.randomTokenGenerator = randomTokenGenerator;
        this.urlHelperFactory = urlHelperFactory;
        this.userDataProvider = userDataProvider;
        this.userPrincipalFactory = userPrincipalFactory;
    }

    public async Task<User?> Authenticate(string email, string password)
    {
        using var connection = await this.mySqlConnectionSource.OpenConnection();

        var user = await this.userDataProvider.FindUserByEmail(connection, email);

        if (user == null)
        {
            AuthenticateLogMessages.UnrecognizedEmail(this.logger, email, null);

            await this.authenticationEventDataProvider.LogEvent(
                connection, "authentication_failure:unrecognized_email", null, email);

            return null;
        }

        if (user.HashedPassword == null)
        {
            AuthenticateLogMessages.NoPasswordSet(this.logger, user.Id, user.Email, null);

            await this.authenticationEventDataProvider.LogEvent(
                connection, "authentication_failure:no_password_set", user.Id, email);

            return null;
        }

        if (!this.VerifyPassword(user, user.HashedPassword, password))
        {
            AuthenticateLogMessages.IncorrectPassword(this.logger, user.Id, user.Email, null);

            await this.authenticationEventDataProvider.LogEvent(
                connection, "authentication_failure:incorrect_password", user.Id, email);

            return null;
        }

        AuthenticateLogMessages.Success(this.logger, user.Id, user.Email, null);

        await this.authenticationEventDataProvider.LogEvent(
            connection, "authentication_success", user.Id, email);

        return user;
    }

    public async Task<bool> ChangePassword(
        HttpContext httpContext, string currentPassword, string newPassword)
    {
        using var connection = await this.mySqlConnectionSource.OpenConnection();

        var user = httpContext.GetCurrentUser()!;

        if (user.HashedPassword == null)
        {
            await this.authenticationEventDataProvider.LogEvent(
                connection, "password_change_failure:no_password_set", user.Id);

            throw new InvalidOperationException(
                $"User {user.Id} ({user.Email}) does not have a password.");
        }

        if (!this.VerifyPassword(user, user.HashedPassword, currentPassword))
        {
            ChangePasswordLogMessages.IncorrectPassword(
                this.logger, user.Id, user.Email, null);

            await this.authenticationEventDataProvider.LogEvent(
                connection, "password_change_failure:incorrect_password", user.Id);

            return false;
        }

        var newSecurityStamp = await this.SetPassword(connection, user, newPassword);

        ChangePasswordLogMessages.Success(this.logger, user.Id, user.Email, null);

        await this.authenticationEventDataProvider.LogEvent(
            connection, "password_change_success", user.Id);

        await this.authenticationMailer.SendPasswordChangeNotification(user.Email);

        await this.SignInUser(httpContext, user with { SecurityStamp = newSecurityStamp });

        return true;
    }

    public async Task<bool> PasswordResetTokenIsValid(string token)
    {
        using var connection = await this.mySqlConnectionSource.OpenConnection();

        var userId = await this.ValidatePasswordResetToken(connection, token);

        if (userId.HasValue)
        {
            PasswordResetTokenIsValidLogMessages.Valid(
                this.logger, RedactToken(token), userId.Value, null);
        }
        else
        {
            PasswordResetTokenIsValidLogMessages.Invalid(this.logger, RedactToken(token), null);

            await this.authenticationEventDataProvider.LogEvent(
                connection, "password_reset_failure:invalid_token");
        }

        return userId.HasValue;
    }

    public async Task<User> ResetPassword(string token, string newPassword)
    {
        using var connection = await this.mySqlConnectionSource.OpenConnection();

        var userId = await this.ValidatePasswordResetToken(connection, token);

        if (!userId.HasValue)
        {
            ResetPasswordLogMessages.InvalidToken(this.logger, RedactToken(token), null);

            await this.authenticationEventDataProvider.LogEvent(
                connection, "password_reset_failure:invalid_token");

            throw new InvalidTokenException("Password reset token is invalid");
        }

        var user = await this.userDataProvider.GetUser(connection, userId.Value);

        var newSecurityStamp = await this.SetPassword(connection, user, newPassword);

        ResetPasswordLogMessages.Success(this.logger, userId.Value, RedactToken(token), null);

        await this.authenticationEventDataProvider.LogEvent(
            connection, "password_reset_success", userId.Value);

        await this.authenticationMailer.SendPasswordChangeNotification(user.Email);

        return user with { SecurityStamp = newSecurityStamp };
    }

    public async Task SendPasswordResetLink(ActionContext actionContext, string email)
    {
        using var connection = await this.mySqlConnectionSource.OpenConnection();

        var user = await this.userDataProvider.FindUserByEmail(connection, email);

        if (user == null)
        {
            SendPasswordResetLinkLogMessages.UnrecognizedEmail(this.logger, email, null);

            await this.authenticationEventDataProvider.LogEvent(
                connection, "password_reset_failure:unrecognized_email", null, email);

            return;
        }

        email = user.Email;

        var token = this.randomTokenGenerator.Generate(12);

        await this.passwordResetTokenDataProvider.InsertToken(connection, user.Id, token);

        var urlHelper = this.urlHelperFactory.GetUrlHelper(actionContext);
        var link = urlHelper.Link("ResetPassword", new { token })!;

        await this.authenticationMailer.SendPasswordResetLink(email, link);

        SendPasswordResetLinkLogMessages.Success(this.logger, user.Id, email, null);

        await this.authenticationEventDataProvider.LogEvent(
            connection, "password_reset_link_sent", user.Id, email);
    }

    public async Task SignIn(HttpContext httpContext, User user)
    {
        await this.SignInUser(httpContext, user);

        httpContext.SetCurrentUser(user);

        SignInLogMessages.SignedIn(this.logger, user.Id, user.Email, null);

        using var connection = await this.mySqlConnectionSource.OpenConnection();

        await this.authenticationEventDataProvider.LogEvent(connection, "sign_in", user.Id);
    }

    public async Task SignOut(HttpContext httpContext)
    {
        await this.SignOutCurrentUser(httpContext);

        var userId = httpContext.User.GetUserId();

        if (userId.HasValue)
        {
            var email = httpContext.User.FindFirstValue(ClaimTypes.Email);

            SignOutLogMessages.SignedOut(this.logger, userId.Value, email, null);

            using var connection = await this.mySqlConnectionSource.OpenConnection();

            await this.authenticationEventDataProvider.LogEvent(connection, "sign_out", userId);
        }
    }

    public async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        var principal = context.Principal;

        if (principal == null)
        {
            return;
        }

        var userId = principal.GetUserId();

        if (!userId.HasValue)
        {
            return;
        }

        User user;

        using (var connection = await this.mySqlConnectionSource.OpenConnection())
        {
            user = await this.userDataProvider.GetUser(connection, userId.Value);
        }

        var securityStamp = principal.FindFirstValue(CustomClaimTypes.SecurityStamp);

        if (string.Equals(securityStamp, user.SecurityStamp, StringComparison.Ordinal))
        {
            context.HttpContext.SetCurrentUser(user);

            ValidatePrincipalLogMessages.Success(this.logger, user.Id, user.Email, null);
        }
        else
        {
            ValidatePrincipalLogMessages.IncorrectSecurityStamp(
                this.logger, user.Id, user.Email, null);

            context.RejectPrincipal();

            await this.SignOutCurrentUser(context.HttpContext);
        }
    }

    private static string RedactToken(string token) => $"{token[..6]}…";

    private async Task<string> SetPassword(
        MySqlConnection connection, User user, string newPassword)
    {
        var hashedPassword = this.passwordHasher.HashPassword(user, newPassword);

        var securityStamp = this.randomTokenGenerator.Generate(2);

        await this.userDataProvider.UpdatePassword(
            connection, user.Id, hashedPassword, securityStamp);

        await this.passwordResetTokenDataProvider.DeleteTokensForUser(connection, user.Id);

        return securityStamp;
    }

    private async Task SignInUser(HttpContext httpContext, User user)
    {
        var principal = this.userPrincipalFactory.Create(
            user, CookieAuthenticationDefaults.AuthenticationScheme);

        await this.authenticationService.SignInAsync(
            httpContext, CookieAuthenticationDefaults.AuthenticationScheme, principal, null);
    }

    private Task SignOutCurrentUser(HttpContext httpContext) =>
        this.authenticationService.SignOutAsync(
            httpContext, CookieAuthenticationDefaults.AuthenticationScheme, null);

    private async Task<long?> ValidatePasswordResetToken(
        MySqlConnection connection, string token)
    {
        await this.passwordResetTokenDataProvider.DeleteExpiredTokens(
            connection, this.clock.UtcNow.AddDays(-1));

        return await this.passwordResetTokenDataProvider.GetUserIdForToken(connection, token);
    }

    private bool VerifyPassword(User user, string hashedPassword, string password) =>
        this.passwordHasher.VerifyHashedPassword(user, hashedPassword, password) ==
            PasswordVerificationResult.Success;

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

    private static class SignInLogMessages
    {
        public static readonly Action<ILogger, long, string, Exception?> SignedIn =
            LoggerMessage.Define<long, string>(
                LogLevel.Information, 212, "User {UserId} ({Email}) signed in");
    }

    private static class SignOutLogMessages
    {
        public static readonly Action<ILogger, long, string?, Exception?> SignedOut =
            LoggerMessage.Define<long, string?>(
                LogLevel.Information, 213, "User {UserId} ({Email}) signed out");
    }

    private static class ValidatePrincipalLogMessages
    {
        public static readonly Action<ILogger, long, string, Exception?> IncorrectSecurityStamp =
            LoggerMessage.Define<long, string>(
                LogLevel.Information, 214, "Incorrect security stamp for user {UserId} ({Email})");

        public static readonly Action<ILogger, long, string, Exception?> Success =
            LoggerMessage.Define<long, string>(
                LogLevel.Debug,
                215,
                "Principal successfully validated for user {UserId} ({Email})");
    }
}
