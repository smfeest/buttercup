using System;
using System.Data.Common;
using System.Globalization;
using System.Security.Claims;
using System.Threading.Tasks;
using Buttercup.DataAccess;
using Buttercup.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;

namespace Buttercup.Web.Authentication
{
    public class AuthenticationManager : IAuthenticationManager
    {
        private readonly IAuthenticationEventDataProvider authenticationEventDataProvider;
        private readonly IAuthenticationMailer authenticationMailer;
        private readonly IAuthenticationService authenticationService;
        private readonly IClock clock;
        private readonly IDbConnectionSource dbConnectionSource;
        private readonly ILogger<AuthenticationManager> logger;
        private readonly IPasswordHasher<User?> passwordHasher;
        private readonly IPasswordResetTokenDataProvider passwordResetTokenDataProvider;
        private readonly IRandomTokenGenerator randomTokenGenerator;
        private readonly IUrlHelperFactory urlHelperFactory;
        private readonly IUserDataProvider userDataProvider;

        public AuthenticationManager(
            IAuthenticationEventDataProvider authenticationEventDataProvider,
            IAuthenticationMailer authenticationMailer,
            IAuthenticationService authenticationService,
            IClock clock,
            IDbConnectionSource dbConnectionSource,
            ILogger<AuthenticationManager> logger,
            IPasswordHasher<User?> passwordHasher,
            IPasswordResetTokenDataProvider passwordResetTokenDataProvider,
            IRandomTokenGenerator randomTokenGenerator,
            IUrlHelperFactory urlHelperFactory,
            IUserDataProvider userDataProvider)
        {
            this.authenticationEventDataProvider = authenticationEventDataProvider;
            this.authenticationMailer = authenticationMailer;
            this.authenticationService = authenticationService;
            this.clock = clock;
            this.dbConnectionSource = dbConnectionSource;
            this.logger = logger;
            this.passwordHasher = passwordHasher;
            this.passwordResetTokenDataProvider = passwordResetTokenDataProvider;
            this.randomTokenGenerator = randomTokenGenerator;
            this.urlHelperFactory = urlHelperFactory;
            this.userDataProvider = userDataProvider;
        }

        public async Task<User?> Authenticate(string email, string password)
        {
            using var connection = await this.dbConnectionSource.OpenConnection();

            var user = await this.userDataProvider.FindUserByEmail(connection, email);

            if (user == null)
            {
                this.logger.LogInformation(
                    "Authentication failed; no user with email {email}", email);

                await this.authenticationEventDataProvider.LogEvent(
                    connection,
                    this.clock.UtcNow,
                    "authentication_failure:unrecognized_email",
                    null,
                    email);

                return null;
            }

            if (user.HashedPassword == null)
            {
                this.logger.LogInformation(
                    "Authentication failed; no password set for user {userId} ({email})",
                    user.Id,
                    user.Email);

                await this.authenticationEventDataProvider.LogEvent(
                    connection,
                    this.clock.UtcNow,
                    "authentication_failure:no_password_set",
                    user.Id,
                    email);

                return null;
            }

            if (!this.VerifyPassword(user, password))
            {
                this.logger.LogInformation(
                    "Authentication failed; incorrect password for user {userId} ({email})",
                    user.Id,
                    user.Email);

                await this.authenticationEventDataProvider.LogEvent(
                    connection,
                    this.clock.UtcNow,
                    "authentication_failure:incorrect_password",
                    user.Id,
                    email);

                return null;
            }

            this.logger.LogInformation(
                "User {userId} ({email}) successfully authenticated", user.Id, user.Email);

            await this.authenticationEventDataProvider.LogEvent(
                connection, this.clock.UtcNow, "authentication_success", user.Id, email);

            return user;
        }

        public async Task<bool> ChangePassword(
            HttpContext httpContext, string currentPassword, string newPassword)
        {
            using var connection = await this.dbConnectionSource.OpenConnection();

            var user = httpContext.GetCurrentUser()!;

            if (user.HashedPassword == null)
            {
                await this.authenticationEventDataProvider.LogEvent(
                    connection,
                    this.clock.UtcNow,
                    "password_change_failure:no_password_set",
                    user.Id);

                throw new InvalidOperationException(
                    $"User {user.Id} ({user.Email}) does not have a password.");
            }

            if (!this.VerifyPassword(user, currentPassword))
            {
                this.logger.LogInformation(
                    "Password change denied for user {userId} ({email}); current password is incorrect",
                    user.Id,
                    user.Email);

                await this.authenticationEventDataProvider.LogEvent(
                    connection,
                    this.clock.UtcNow,
                    "password_change_failure:incorrect_password",
                    user.Id);

                return false;
            }

            user.SecurityStamp = await this.SetPassword(connection, user.Id, newPassword);

            this.logger.LogInformation(
                "Password successfully changed for user {userId} ({email})",
                user.Id,
                user.Email);

            await this.authenticationEventDataProvider.LogEvent(
                connection, this.clock.UtcNow, "password_change_success", user.Id);

            await this.authenticationMailer.SendPasswordChangeNotification(user.Email!);

            await this.SignInUser(httpContext, user);

            return true;
        }

        public async Task<bool> PasswordResetTokenIsValid(string token)
        {
            using var connection = await this.dbConnectionSource.OpenConnection();

            var userId = await this.ValidatePasswordResetToken(connection, token);

            if (userId.HasValue)
            {
                this.logger.LogDebug(
                    "Password reset token '{token}' is valid and belongs to user {userId}",
                    RedactToken(token),
                    userId);
            }
            else
            {
                this.logger.LogDebug(
                    "Password reset token '{token}' is no longer valid", RedactToken(token));

                await this.authenticationEventDataProvider.LogEvent(
                    connection, this.clock.UtcNow, "password_reset_failure:invalid_token");
            }

            return userId.HasValue;
        }

        public async Task<User> ResetPassword(string token, string newPassword)
        {
            using var connection = await this.dbConnectionSource.OpenConnection();

            var userId = await this.ValidatePasswordResetToken(connection, token);

            if (!userId.HasValue)
            {
                this.logger.LogInformation(
                    "Unable to reset password; password reset token {token} is invalid",
                    RedactToken(token));

                await this.authenticationEventDataProvider.LogEvent(
                    connection, this.clock.UtcNow, "password_reset_failure:invalid_token");

                throw new InvalidTokenException("Password reset token is invalid");
            }

            await this.SetPassword(connection, userId.Value, newPassword);

            this.logger.LogInformation(
                "Password reset for user {userId} using token {token}",
                userId,
                RedactToken(token));

            await this.authenticationEventDataProvider.LogEvent(
                connection, this.clock.UtcNow, "password_reset_success", userId.Value);

            var user = await this.userDataProvider.GetUser(connection, userId.Value);

            await this.authenticationMailer.SendPasswordChangeNotification(user.Email!);

            return user;
        }

        public async Task SendPasswordResetLink(ActionContext actionContext, string email)
        {
            using var connection = await this.dbConnectionSource.OpenConnection();

            var user = await this.userDataProvider.FindUserByEmail(connection, email);

            if (user == null)
            {
                this.logger.LogInformation(
                    "Unable to send password reset link; No user with email {email}", email);

                await this.authenticationEventDataProvider.LogEvent(
                    connection,
                    this.clock.UtcNow,
                    "password_reset_failure:unrecognized_email",
                    null,
                    email);

                return;
            }

            email = user.Email!;

            var token = this.randomTokenGenerator.Generate(12);

            await this.passwordResetTokenDataProvider.InsertToken(
                connection, user.Id, token, this.clock.UtcNow);

            var urlHelper = this.urlHelperFactory.GetUrlHelper(actionContext);
            var link = urlHelper.Link("ResetPassword", new { token })!;

            try
            {
                await this.authenticationMailer.SendPasswordResetLink(email, link);

                this.logger.LogInformation(
                    "Password reset link sent to user {userId} ({email})",
                    user.Id,
                    email);

                await this.authenticationEventDataProvider.LogEvent(
                    connection, this.clock.UtcNow, "password_reset_link_sent", user.Id, email);
            }
#pragma warning disable CA1031
            catch (Exception e)
            {
                this.logger.LogError(
                    e,
                    "Error sending password reset link to user {userId} ({email})",
                    user.Id,
                    email);
            }
#pragma warning restore CA1031
        }

        public async Task SignIn(HttpContext httpContext, User user)
        {
            await this.SignInUser(httpContext, user);

            httpContext.SetCurrentUser(user);

            this.logger.LogInformation("User {userId} ({email}) signed in", user.Id, user.Email);

            using var connection = await this.dbConnectionSource.OpenConnection();

            await this.authenticationEventDataProvider.LogEvent(
                connection, this.clock.UtcNow, "sign_in", user.Id);
        }

        public async Task SignOut(HttpContext httpContext)
        {
            await this.SignOutCurrentUser(httpContext);

            var userId = GetUserId(httpContext.User);

            if (userId.HasValue)
            {
                var email = httpContext.User.FindFirstValue(ClaimTypes.Email);

                this.logger.LogInformation("User {userId} ({email}) signed out", userId, email);

                using var connection = await this.dbConnectionSource.OpenConnection();

                await this.authenticationEventDataProvider.LogEvent(
                    connection, this.clock.UtcNow, "sign_out", userId);
            }
        }

        public async Task ValidatePrincipal(CookieValidatePrincipalContext context)
        {
            var principal = context.Principal;

            var userId = GetUserId(principal);

            if (userId.HasValue)
            {
                User user;

                using (var connection = await this.dbConnectionSource.OpenConnection())
                {
                    user = await this.userDataProvider.GetUser(connection, userId.Value);
                }

                var securityStamp = principal.FindFirstValue(CustomClaimTypes.SecurityStamp);

                if (string.Equals(securityStamp, user.SecurityStamp, StringComparison.Ordinal))
                {
                    context.HttpContext.SetCurrentUser(user);

                    this.logger.LogDebug(
                        "Principal successfully validated for user {userId} ({email})",
                        user.Id,
                        user.Email);
                }
                else
                {
                    this.logger.LogInformation(
                        "Incorrect security stamp for user {userId} ({email})",
                        user.Id,
                        user.Email);

                    context.RejectPrincipal();

                    await this.SignOutCurrentUser(context.HttpContext);
                }
            }
        }

        private static long? GetUserId(ClaimsPrincipal? principal)
        {
            var claimValue = principal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (claimValue == null)
            {
                return null;
            }

            return long.Parse(claimValue, CultureInfo.InvariantCulture);
        }

        private static string RedactToken(string token) => $"{token.Substring(0, 6)}…";

        private async Task<string> SetPassword(
            DbConnection connection, long userId, string newPassword)
        {
            var hashedPassword = this.passwordHasher.HashPassword(null, newPassword);

            var securityToken = this.randomTokenGenerator.Generate(2);

            await this.userDataProvider.UpdatePassword(
                connection, userId, hashedPassword, securityToken, this.clock.UtcNow);

            await this.passwordResetTokenDataProvider.DeleteTokensForUser(connection, userId);

            return securityToken;
        }

        private async Task SignInUser(HttpContext httpContext, User user)
        {
            var claims = new Claim[]
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString(CultureInfo.InvariantCulture)),
                new(ClaimTypes.Email, user.Email!),
                new(CustomClaimTypes.SecurityStamp, user.SecurityStamp!),
            };

            var principal = new ClaimsPrincipal(new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Email, null));

            await this.authenticationService.SignInAsync(
                httpContext, CookieAuthenticationDefaults.AuthenticationScheme, principal, null);
        }

        private Task SignOutCurrentUser(HttpContext httpContext) =>
            this.authenticationService.SignOutAsync(
                httpContext, CookieAuthenticationDefaults.AuthenticationScheme, null);

        private async Task<long?> ValidatePasswordResetToken(DbConnection connection, string token)
        {
            await this.passwordResetTokenDataProvider.DeleteExpiredTokens(
                connection, this.clock.UtcNow.AddDays(-1));

            return await this.passwordResetTokenDataProvider.GetUserIdForToken(connection, token);
        }

        private bool VerifyPassword(User user, string password) =>
            this.passwordHasher.VerifyHashedPassword(user, user.HashedPassword, password) ==
                PasswordVerificationResult.Success;
    }
}
