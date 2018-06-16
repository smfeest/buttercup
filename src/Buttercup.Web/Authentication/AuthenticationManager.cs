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
            IAuthenticationService authenticationService,
            IDbConnectionSource dbConnectionSource,
            ILogger<AuthenticationManager> logger,
            IPasswordHasher<User> passwordHasher,
            IUserDataProvider userDataProvider)
        {
            this.AuthenticationService = authenticationService;
            this.DbConnectionSource = dbConnectionSource;
            this.Logger = logger;
            this.PasswordHasher = passwordHasher;
            this.UserDataProvider = userDataProvider;
        }

        public IAuthenticationService AuthenticationService { get; }

        public IDbConnectionSource DbConnectionSource { get; }

        public ILogger<AuthenticationManager> Logger { get; }

        public IPasswordHasher<User> PasswordHasher { get; }

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
    }
}
