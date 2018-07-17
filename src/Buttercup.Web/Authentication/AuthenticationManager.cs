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
            IUserDataProvider userDataProvider)
        {
            this.AuthenticationMailer = authenticationMailer;
            this.AuthenticationService = authenticationService;
            this.DbConnectionSource = dbConnectionSource;
            this.Logger = logger;
            this.PasswordHasher = passwordHasher;
            this.PasswordResetTokenDataProvider = passwordResetTokenDataProvider;
            this.RandomTokenGenerator = randomTokenGenerator;
            this.UserDataProvider = userDataProvider;
        }

        public IAuthenticationMailer AuthenticationMailer { get; }

        public IAuthenticationService AuthenticationService { get; }

        public IDbConnectionSource DbConnectionSource { get; }

        public ILogger<AuthenticationManager> Logger { get; }

        public IPasswordHasher<User> PasswordHasher { get; }

        public IPasswordResetTokenDataProvider PasswordResetTokenDataProvider { get; }

        public IRandomTokenGenerator RandomTokenGenerator { get; }

        public IUserDataProvider UserDataProvider { get; }

        public async Task<User> Authenticate(string email, string password)
        {
            using (var connection = await this.DbConnectionSource.OpenConnection())
            {
                var user = await this.UserDataProvider.FindUserByEmail(connection, email);

                if (user == null)
                {
                    this.Logger.LogInformation(
                        "Authentication failure: No user with email {email}",
                        email);

                    return null;
                }

                if (user.HashedPassword == null)
                {
                    this.Logger.LogInformation(
                        "Authentication failure: No password set for user {userId} ({email})",
                        user.Id,
                        user.Email);

                    return null;
                }

                if (this.PasswordHasher.VerifyHashedPassword(user, user.HashedPassword, password) !=
                    PasswordVerificationResult.Success)
                {
                    this.Logger.LogInformation(
                        "Authentication failure: Incorrect password for user {userId} ({email})",
                        user.Id,
                        user.Email);

                    return null;
                }

                this.Logger.LogInformation(
                    "User {userId} ({email}) successfully authenticated",
                    user.Id,
                    user.Email);

                return user;
            }
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

                var hashedPassword = this.PasswordHasher.HashPassword(null, newPassword);

                await this.UserDataProvider.UpdatePassword(
                    connection, userId.Value, hashedPassword);

                await this.PasswordResetTokenDataProvider.DeleteTokensForUser(
                    connection, userId.Value);

                this.Logger.LogInformation(
                    "Password reset for user {userId} using token {token}",
                    userId,
                    RedactToken(token));

                return await this.UserDataProvider.GetUser(connection, userId.Value);
            }
        }

        public async Task SendPasswordResetLink(string email)
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

                try
                {
                    await this.AuthenticationMailer.SendPasswordResetLink(email, token);

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

        private async Task<long?> ValidatePasswordResetToken(DbConnection connection, string token)
        {
            await this.PasswordResetTokenDataProvider.DeleteExpiredTokens(connection);

            return await this.PasswordResetTokenDataProvider.GetUserIdForToken(connection, token);
        }
    }
}
