using System.Security.Claims;
using Buttercup.DataAccess;
using Buttercup.EntityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Buttercup.Security;

internal sealed class CookieAuthenticationService : ICookieAuthenticationService
{
    private readonly IAuthenticationService authenticationService;
    private readonly IDbContextFactory<AppDbContext> dbContextFactory;
    private readonly ILogger<CookieAuthenticationService> logger;
    private readonly ISecurityEventDataProvider securityEventDataProvider;
    private readonly IUserDataProvider userDataProvider;
    private readonly IUserPrincipalFactory userPrincipalFactory;

    public CookieAuthenticationService(
        IAuthenticationService authenticationService,
        IDbContextFactory<AppDbContext> dbContextFactory,
        ILogger<CookieAuthenticationService> logger,
        ISecurityEventDataProvider securityEventDataProvider,
        IUserDataProvider userDataProvider,
        IUserPrincipalFactory userPrincipalFactory)
    {
        this.authenticationService = authenticationService;
        this.dbContextFactory = dbContextFactory;
        this.logger = logger;
        this.securityEventDataProvider = securityEventDataProvider;
        this.userDataProvider = userDataProvider;
        this.userPrincipalFactory = userPrincipalFactory;
    }

    public async Task<bool> RefreshPrincipal(HttpContext httpContext)
    {
        var authenticateResult = await this.authenticationService.AuthenticateAsync(
            httpContext, CookieAuthenticationDefaults.AuthenticationScheme);

        if (!authenticateResult.Succeeded)
        {
            return false;
        }

        using var dbContext = this.dbContextFactory.CreateDbContext();

        var user = await this.userDataProvider.GetUser(
            dbContext, authenticateResult.Principal.GetUserId());

        await this.SignInUser(httpContext, user, authenticateResult.Properties);

        return true;
    }

    public async Task SignIn(HttpContext httpContext, User user)
    {
        await this.SignInUser(httpContext, user);

        using var dbContext = this.dbContextFactory.CreateDbContext();

        await this.securityEventDataProvider.LogEvent(
            dbContext, "sign_in", httpContext.Connection.RemoteIpAddress, user.Id);

        SignInLogMessages.SignedIn(this.logger, user.Id, user.Email, null);
    }

    public async Task SignOut(HttpContext httpContext)
    {
        await this.SignOutCurrentUser(httpContext);

        var userId = httpContext.User.TryGetUserId();

        if (userId.HasValue)
        {
            using var dbContext = this.dbContextFactory.CreateDbContext();

            await this.securityEventDataProvider.LogEvent(
                dbContext, "sign_out", httpContext.Connection.RemoteIpAddress, userId);

            var email = httpContext.User.FindFirstValue(ClaimTypes.Email);

            SignOutLogMessages.SignedOut(this.logger, userId.Value, email, null);
        }
    }

    private async Task SignInUser(
        HttpContext httpContext, User user, AuthenticationProperties? properties = null)
    {
        var principal = this.userPrincipalFactory.Create(
            user, CookieAuthenticationDefaults.AuthenticationScheme);

        await this.authenticationService.SignInAsync(
            httpContext, CookieAuthenticationDefaults.AuthenticationScheme, principal, properties);
    }

    private Task SignOutCurrentUser(HttpContext httpContext) =>
        this.authenticationService.SignOutAsync(
            httpContext, CookieAuthenticationDefaults.AuthenticationScheme, null);

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
}
