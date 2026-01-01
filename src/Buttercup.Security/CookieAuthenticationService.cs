using System.Net;
using System.Security.Claims;
using Buttercup.EntityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Buttercup.Security;

internal sealed partial class CookieAuthenticationService(
    IAuthenticationService authenticationService,
    IClaimsIdentityFactory claimsIdentityFactory,
    IDbContextFactory<AppDbContext> dbContextFactory,
    ILogger<CookieAuthenticationService> logger,
    TimeProvider timeProvider)
    : ICookieAuthenticationService
{
    private readonly IAuthenticationService authenticationService = authenticationService;
    private readonly IClaimsIdentityFactory claimsIdentityFactory = claimsIdentityFactory;
    private readonly IDbContextFactory<AppDbContext> dbContextFactory = dbContextFactory;
    private readonly ILogger<CookieAuthenticationService> logger = logger;
    private readonly TimeProvider timeProvider = timeProvider;

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

        await this.InsertUserAuditEntry(
            UserAuditOperation.SignIn, user.Id, httpContext.Connection.RemoteIpAddress);

        this.LogSignedIn(user.Id, user.Email);
    }

    public async Task SignOut(HttpContext httpContext)
    {
        await this.SignOutCurrentUser(httpContext);

        var userId = httpContext.User.TryGetUserId();

        if (userId.HasValue)
        {
            await this.InsertUserAuditEntry(
                UserAuditOperation.SignOut, userId.Value, httpContext.Connection.RemoteIpAddress);

            var email = httpContext.User.FindFirstValue(ClaimTypes.Email);

            this.LogSignedOut(userId.Value, email);
        }
    }

    private async Task InsertUserAuditEntry(
        UserAuditOperation operation, long userId, IPAddress? ipAddress)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        dbContext.UserAuditEntries.Add(new()
        {
            Time = this.timeProvider.GetUtcDateTimeNow(),
            Operation = operation,
            TargetId = userId,
            ActorId = userId,
            IpAddress = ipAddress,
        });

        await dbContext.SaveChangesAsync();
    }

    private async Task SignInUser(
        HttpContext httpContext, User user, AuthenticationProperties? properties = null)
    {
        var userIdentity = this.claimsIdentityFactory.CreateIdentityForUser(
            user, CookieAuthenticationDefaults.AuthenticationScheme);

        await this.authenticationService.SignInAsync(
            httpContext,
            CookieAuthenticationDefaults.AuthenticationScheme,
            new(userIdentity),
            properties);
    }

    private Task SignOutCurrentUser(HttpContext httpContext) =>
        this.authenticationService.SignOutAsync(
            httpContext, CookieAuthenticationDefaults.AuthenticationScheme, null);

    [LoggerMessage(
        EventId = 1,
        EventName = "SignedIn",
        Level = LogLevel.Information,
        Message = "User {UserId} ({Email}) signed in")]
    private partial void LogSignedIn(long userId, string email);

    [LoggerMessage(
        EventId = 2,
        EventName = "SignedOut",
        Level = LogLevel.Information,
        Message = "User {UserId} ({Email}) signed out")]
    private partial void LogSignedOut(long userId, string? email);
}
