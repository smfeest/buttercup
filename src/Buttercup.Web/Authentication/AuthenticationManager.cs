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

                    return null;
                }

                if (user.HashedPassword == null)
                {
                    this.Logger.LogInformation(
                        "Authentication failed; no password set for user {userId} ({email})",
                        user.Id,
                        user.Email);

                    return null;
                }

                if (!this.VerifyPassword(user, password))
                {
                    this.Logger.LogInformation(
                        "Authentication failed; incorrect password for user {userId} ({email})",
                        user.Id,
                        user.Email);

                    return null;
                }

                this.Logger.LogInformation(
                    "User {userId} ({email}) successfully authenticated", user.Id, user.Email);

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
                    throw new InvalidOperationException(
                        $"User {user.Id} ({user.Email}) does not have a password.");
                }

                if (!this.VerifyPassword(user, currentPassword))
                {
                    this.Logger.LogInformation(
                        "Password change denied for user {userId} ({email}); current password is incorrect",
                        user.Id,
                        user.Email);

                    return false;
                }

                await this.SetPassword(connection, user.Id, newPassword);

                this.Logger.LogInformation(
                    "Password successfully changed for user {userId} ({email})",
                    user.Id,
                    user.Email);

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

            if (httpContext.User.Identity.IsAuthenticated)
            {
                var userId = long.Parse(
                    httpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value,
                    CultureInfo.InvariantCulture);

                using (var connection = await this.DbConnectionSource.OpenConnection())
                {
                    user = await this.UserDataProvider.GetUser(connection, userId);
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

                    throw new InvalidTokenException("Password reset token is invalid");
                }

                await this.SetPassword(connection, userId.Value, newPassword);

                this.Logger.LogInformation(
                    "Password reset for user {userId} using token {token}",
                    userId,
                    RedactToken(token));

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
                    return;
                }

                email = user.Email;

                var token = this.RandomTokenGenerator.Generate();

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
            };

            var principal = new ClaimsPrincipal(new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Email, null));

            await this.AuthenticationService.SignInAsync(
                httpContext, CookieAuthenticationDefaults.AuthenticationScheme, principal, null);

            this.Logger.LogInformation("User {userId} ({email}) signed in", user.Id, user.Email);
        }

        public async Task SignOut(HttpContext httpContext)
        {
            var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            await this.AuthenticationService.SignOutAsync(httpContext, null, null);

            if (userId != null)
            {
                var email = httpContext.User.FindFirstValue(ClaimTypes.Email);

                this.Logger.LogInformation("User {userId} ({email}) signed out", userId, email);
            }
        }

        private static string RedactToken(string token) => $"{token.Substring(0, 6)}…";

        private async Task SetPassword(DbConnection connection, long userId, string newPassword)
        {
            var hashedPassword = this.PasswordHasher.HashPassword(null, newPassword);

            await this.UserDataProvider.UpdatePassword(connection, userId, hashedPassword);

            await this.PasswordResetTokenDataProvider.DeleteTokensForUser(connection, userId);
        }

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
