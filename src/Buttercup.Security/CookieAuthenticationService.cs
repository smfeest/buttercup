using System.Net;
using System.Security.Claims;
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
    private readonly IClock clock;
    private readonly IDbContextFactory<AppDbContext> dbContextFactory;
    private readonly ILogger<CookieAuthenticationService> logger;
    private readonly IUserPrincipalFactory userPrincipalFactory;

    public CookieAuthenticationService(
        IAuthenticationService authenticationService,
        IClock clock,
        IDbContextFactory<AppDbContext> dbContextFactory,
        ILogger<CookieAuthenticationService> logger,
        IUserPrincipalFactory userPrincipalFactory)
    {
        this.authenticationService = authenticationService;
        this.clock = clock;
        this.dbContextFactory = dbContextFactory;
        this.logger = logger;
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

        var user = await dbContext.Users.GetAsync(authenticateResult.Principal.GetUserId());

        await this.SignInUser(httpContext, user, authenticateResult.Properties);

        return true;
    }

    public async Task SignIn(HttpContext httpContext, User user)
    {
        await this.SignInUser(httpContext, user);

        await this.InsertSecurityEvent("sign_in", httpContext.Connection.RemoteIpAddress, user.Id);

        SignInLogMessages.SignedIn(this.logger, user.Id, user.Email, null);
    }

    public async Task SignOut(HttpContext httpContext)
    {
        await this.SignOutCurrentUser(httpContext);

        var userId = httpContext.User.TryGetUserId();

        if (userId.HasValue)
        {
            await this.InsertSecurityEvent(
                "sign_out", httpContext.Connection.RemoteIpAddress, userId.Value);

            var email = httpContext.User.FindFirstValue(ClaimTypes.Email);

            SignOutLogMessages.SignedOut(this.logger, userId.Value, email, null);
        }
    }

    private async Task InsertSecurityEvent(string eventName, IPAddress? ipAddress, long userId)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        dbContext.SecurityEvents.Add(new()
        {
            Time = this.clock.UtcNow,
            Event = eventName,
            IpAddress = ipAddress,
            UserId = userId,
        });

        await dbContext.SaveChangesAsync();
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
