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
        public AuthenticationManager(
            IAuthenticationEventDataProvider authenticationEventDataProvider,
            IAuthenticationMailer authenticationMailer,
            IAuthenticationService authenticationService,
            IDbConnectionSource dbConnectionSource,
            ILogger<AuthenticationManager> logger,
            IPasswordHasher<User> passwordHasher,
            IPasswordResetTokenDataProvider passwordResetTokenDataProvider,
            IRandomTokenGenerator randomTokenGenerator,
            IUrlHelperFactory urlHelperFactory,
            IUserDataProvider userDataProvider)
        {
            this.AuthenticationEventDataProvider = authenticationEventDataProvider;
            this.AuthenticationMailer = authenticationMailer;
            this.AuthenticationService = authenticationService;
            this.DbConnectionSource = dbConnectionSource;
            this.Logger = logger;
            this.PasswordHasher = passwordHasher;
            this.PasswordResetTokenDataProvider = passwordResetTokenDataProvider;
            this.RandomTokenGenerator = randomTokenGenerator;
            this.UrlHelperFactory = urlHelperFactory;
            this.UserDataProvider = userDataProvider;
        }

        public IAuthenticationEventDataProvider AuthenticationEventDataProvider { get; }

        public IAuthenticationMailer AuthenticationMailer { get; }

        public IAuthenticationService AuthenticationService { get; }

        public IDbConnectionSource DbConnectionSource { get; }

        public ILogger<AuthenticationManager> Logger { get; }

        public IPasswordHasher<User> PasswordHasher { get; }

        public IPasswordResetTokenDataProvider PasswordResetTokenDataProvider { get; }

        public IRandomTokenGenerator RandomTokenGenerator { get; }

        public IUrlHelperFactory UrlHelperFactory { get; }

        public IUserDataProvider UserDataProvider { get; }

        public async Task<User> Authenticate(string email, string password)
        {
            using (var connection = await this.DbConnectionSource.OpenConnection())
            {
                var user = await this.UserDataProvider.FindUserByEmail(connection, email);

                if (user == null)
                {
                    this.Logger.LogInformation(
                        "Authentication failed; no user with email {email}", email);

                    await this.AuthenticationEventDataProvider.LogEvent(
                        connection, "authentication_failure:unrecognized_email", null, email);

                    return null;
                }

                if (user.HashedPassword == null)
                {
                    this.Logger.LogInformation(
                        "Authentication failed; no password set for user {userId} ({email})",
                        user.Id,
                        user.Email);

                    await this.AuthenticationEventDataProvider.LogEvent(
                        connection, "authentication_failure:no_password_set", user.Id, email);

                    return null;
                }

                if (!this.VerifyPassword(user, password))
                {
                    this.Logger.LogInformation(
                        "Authentication failed; incorrect password for user {userId} ({email})",
                        user.Id,
                        user.Email);

                    await this.AuthenticationEventDataProvider.LogEvent(
                        connection, "authentication_failure:incorrect_password", user.Id, email);

                    return null;
                }

                this.Logger.LogInformation(
                    "User {userId} ({email}) successfully authenticated", user.Id, user.Email);

                await this.AuthenticationEventDataProvider.LogEvent(
                    connection, "authentication_success", user.Id, email);

                return user;
            }
        }

        public async Task<bool> ChangePassword(
            User user, string currentPassword, string newPassword)
        {
            using (var connection = await this.DbConnectionSource.OpenConnection())
            {
                if (user.HashedPassword == null)
                {
                    await this.AuthenticationEventDataProvider.LogEvent(
                        connection, "password_change_failure:no_password_set", user.Id);

                    throw new InvalidOperationException(
                        $"User {user.Id} ({user.Email}) does not have a password.");
                }

                if (!this.VerifyPassword(user, currentPassword))
                {
                    this.Logger.LogInformation(
                        "Password change denied for user {userId} ({email}); current password is incorrect",
                        user.Id,
                        user.Email);

                    await this.AuthenticationEventDataProvider.LogEvent(
                        connection, "password_change_failure:incorrect_password", user.Id);

                    return false;
                }

                await this.SetPassword(connection, user.Id, newPassword);

                this.Logger.LogInformation(
                    "Password successfully changed for user {userId} ({email})",
                    user.Id,
                    user.Email);

                await this.AuthenticationEventDataProvider.LogEvent(
                    connection, "password_change_success", user.Id);

                await this.AuthenticationMailer.SendPasswordChangeNotification(user.Email);

                return true;
            }
        }

        public async Task<User> GetCurrentUser(HttpContext httpContext)
        {
            object cachedUser;

            if (httpContext.Items.TryGetValue(typeof(User), out cachedUser))
            {
                return (User)cachedUser;
            }

            User user;

            var userId = GetUserId(httpContext.User);

            if (userId.HasValue)
            {
                using (var connection = await this.DbConnectionSource.OpenConnection())
                {
                    user = await this.UserDataProvider.GetUser(connection, userId.Value);
                }
            }
            else
            {
                user = null;
            }

            httpContext.Items.Add(typeof(User), user);

            return user;
        }

        public async Task<bool> PasswordResetTokenIsValid(string token)
        {
            using (var connection = await this.DbConnectionSource.OpenConnection())
            {
                var userId = await this.ValidatePasswordResetToken(connection, token);

                if (userId.HasValue)
                {
                    this.Logger.LogDebug(
                        "Password reset token '{token}' is valid and belongs to user {userId}",
                        RedactToken(token),
                        userId);
                }
                else
                {
                    this.Logger.LogDebug(
                        "Password reset token '{token}' is no longer valid", RedactToken(token));

                    await this.AuthenticationEventDataProvider.LogEvent(
                        connection, "password_reset_failure:invalid_token");
                }

                return userId.HasValue;
            }
        }

        public async Task<User> ResetPassword(string token, string newPassword)
        {
            using (var connection = await this.DbConnectionSource.OpenConnection())
            {
                var userId = await this.ValidatePasswordResetToken(connection, token);

                if (!userId.HasValue)
                {
                    this.Logger.LogInformation(
                        "Unable to reset password; password reset token {token} is invalid",
                        RedactToken(token));

                    await this.AuthenticationEventDataProvider.LogEvent(
                        connection, "password_reset_failure:invalid_token");

                    throw new InvalidTokenException("Password reset token is invalid");
                }

                await this.SetPassword(connection, userId.Value, newPassword);

                this.Logger.LogInformation(
                    "Password reset for user {userId} using token {token}",
                    userId,
                    RedactToken(token));

                await this.AuthenticationEventDataProvider.LogEvent(
                    connection, "password_reset_success", userId.Value);

                var user = await this.UserDataProvider.GetUser(connection, userId.Value);

                await this.AuthenticationMailer.SendPasswordChangeNotification(user.Email);

                return user;
            }
        }

        public async Task SendPasswordResetLink(ActionContext actionContext, string email)
        {
            using (var connection = await this.DbConnectionSource.OpenConnection())
            {
                var user = await this.UserDataProvider.FindUserByEmail(connection, email);

                if (user == null)
                {
                    this.Logger.LogInformation(
                        "Unable to send password reset link; No user with email {email}", email);

                    await this.AuthenticationEventDataProvider.LogEvent(
                        connection, "password_reset_failure:unrecognized_email", null, email);

                    return;
                }

                email = user.Email;

                var token = this.RandomTokenGenerator.Generate(12);

                await this.PasswordResetTokenDataProvider.InsertToken(connection, user.Id, token);

                var urlHelper = this.UrlHelperFactory.GetUrlHelper(actionContext);
                var link = urlHelper.Link("ResetPassword", new { token = token });

                try
                {
                    await this.AuthenticationMailer.SendPasswordResetLink(email, link);

                    this.Logger.LogInformation(
                        "Password reset link sent to user {userId} ({email})",
                        user.Id,
                        email);

                    await this.AuthenticationEventDataProvider.LogEvent(
                        connection, "password_reset_link_sent", user.Id, email);
                }
                catch (Exception e)
                {
                    this.Logger.LogError(
                        e,
                        "Error sending password reset link to user {userId} ({email})",
                        user.Id,
                        email);
                }
            }
        }

        public async Task SignIn(HttpContext httpContext, User user)
        {
            var claims = new Claim[]
            {
                new Claim(
                    ClaimTypes.NameIdentifier, user.Id.ToString(CultureInfo.InvariantCulture)),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(CustomClaimTypes.SecurityStamp, user.SecurityStamp),
            };

            var principal = new ClaimsPrincipal(new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Email, null));

            await this.AuthenticationService.SignInAsync(
                httpContext, CookieAuthenticationDefaults.AuthenticationScheme, principal, null);

            this.Logger.LogInformation("User {userId} ({email}) signed in", user.Id, user.Email);

            using (var connection = await this.DbConnectionSource.OpenConnection())
            {
                await this.AuthenticationEventDataProvider.LogEvent(
                    connection, "sign_in", user.Id);
            }
        }

        public async Task SignOut(HttpContext httpContext)
        {
            await this.SignOutCurrentUser(httpContext);

            var userId = GetUserId(httpContext.User);

            if (userId.HasValue)
            {
                var email = httpContext.User.FindFirstValue(ClaimTypes.Email);

                this.Logger.LogInformation("User {userId} ({email}) signed out", userId, email);

                using (var connection = await this.DbConnectionSource.OpenConnection())
                {
                    await this.AuthenticationEventDataProvider.LogEvent(
                        connection, "sign_out", userId);
                }
            }
        }

        public async Task ValidatePrincipal(CookieValidatePrincipalContext context)
        {
            var principal = context.Principal;

            var userId = GetUserId(principal);

            if (userId.HasValue)
            {
                User user;

                using (var connection = await this.DbConnectionSource.OpenConnection())
                {
                    user = await this.UserDataProvider.GetUser(connection, userId.Value);
                }

                var securityStamp = principal.FindFirstValue(CustomClaimTypes.SecurityStamp);

                if (string.Equals(securityStamp, user.SecurityStamp, StringComparison.Ordinal))
                {
                    this.Logger.LogDebug(
                        "Principal successfully validated for user {userId} ({email})",
                        user.Id,
                        user.Email);
                }
                else
                {
                    this.Logger.LogInformation(
                        "Incorrect security stamp for user {userId} ({email})",
                        user.Id,
                        user.Email);

                    context.RejectPrincipal();

                    await this.SignOutCurrentUser(context.HttpContext);
                }
            }
        }

        private static long? GetUserId(ClaimsPrincipal principal)
        {
            var claimValue = principal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (claimValue == null)
            {
                return null;
            }

            return long.Parse(claimValue, CultureInfo.InvariantCulture);
        }

        private static string RedactToken(string token) => $"{token.Substring(0, 6)}…";

        private async Task SetPassword(DbConnection connection, long userId, string newPassword)
        {
            var hashedPassword = this.PasswordHasher.HashPassword(null, newPassword);

            await this.UserDataProvider.UpdatePassword(connection, userId, hashedPassword);

            await this.PasswordResetTokenDataProvider.DeleteTokensForUser(connection, userId);
        }

        private Task SignOutCurrentUser(HttpContext httpContext) =>
            this.AuthenticationService.SignOutAsync(
                httpContext, CookieAuthenticationDefaults.AuthenticationScheme, null);

        private async Task<long?> ValidatePasswordResetToken(DbConnection connection, string token)
        {
            await this.PasswordResetTokenDataProvider.DeleteExpiredTokens(connection);

            return await this.PasswordResetTokenDataProvider.GetUserIdForToken(connection, token);
        }

        private bool VerifyPassword(User user, string password) =>
            this.PasswordHasher.VerifyHashedPassword(user, user.HashedPassword, password) ==
                PasswordVerificationResult.Success;
    }
}
