using System.Globalization;
using System.Security.Claims;
using Buttercup.Application;
using Buttercup.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Buttercup.Web.Security;

public sealed partial class CookieAuthenticationEventsHandler(
    IAuthenticationService authenticationService,
    IClaimsIdentityFactory claimsIdentityFactory,
    ILogger<CookieAuthenticationEventsHandler> logger,
    IUserManager userManager)
    : CookieAuthenticationEvents
{
    private readonly IAuthenticationService authenticationService = authenticationService;
    private readonly IClaimsIdentityFactory claimsIdentityFactory = claimsIdentityFactory;
    private readonly ILogger<CookieAuthenticationEventsHandler> logger = logger;
    private readonly IUserManager userManager = userManager;

    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        var principal = context.Principal;

        if (principal == null)
        {
            return;
        }

        var userId = principal.TryGetUserId();

        if (!userId.HasValue)
        {
            return;
        }

        var user = await this.userManager.FindUser(userId.Value);

        if (user is null)
        {
            this.LogUserNoLongerExists(userId.Value);
            await this.RejectPrincipalAndSignOut(context);
            return;
        }

        var securityStamp = principal.FindFirstValue(CustomClaimTypes.SecurityStamp);

        if (!string.Equals(securityStamp, user.SecurityStamp, StringComparison.Ordinal))
        {
            this.LogIncorrectSecurityStamp(user.Id, user.Email);
            await this.RejectPrincipalAndSignOut(context);
            return;
        }

        this.LogValidatedPrincipal(user.Id, user.Email);

        var userRevision = principal.FindFirstValue(CustomClaimTypes.UserRevision);

        if (userRevision is null ||
            int.Parse(userRevision, CultureInfo.InvariantCulture) != user.Revision)
        {
            context.ReplacePrincipal(
                new(this.claimsIdentityFactory.CreateIdentityForUser(user, context.Scheme.Name)));
            context.ShouldRenew = true;

            this.LogRefreshedPrincipal(user.Id, user.Email);
        }
    }

    private async Task RejectPrincipalAndSignOut(CookieValidatePrincipalContext context)
    {
        context.RejectPrincipal();

        await this.authenticationService.SignOutAsync(
            context.HttpContext, context.Scheme.Name, null);
    }

    [LoggerMessage(
        EventId = 1,
        EventName = "IncorrectSecurityStamp",
        Level = LogLevel.Information,
        Message = "Incorrect security stamp for user {UserId} ({Email})")]
    private partial void LogIncorrectSecurityStamp(long userId, string email);

    [LoggerMessage(
        EventId = 2,
        EventName = "RefreshedPrincipal",
        Level = LogLevel.Information,
        Message = "Refreshed claims principal for user {UserId} ({Email})")]
    private partial void LogRefreshedPrincipal(long userId, string email);

    [LoggerMessage(
        EventId = 3,
        EventName = "UserNoLongerExists",
        Level = LogLevel.Information,
        Message = "User {UserId} no longer exists")]
    private partial void LogUserNoLongerExists(long userId);

    [LoggerMessage(
        EventId = 4,
        EventName = "ValidatedPrincipal",
        Level = LogLevel.Debug,
        Message = "Successfully validated claims principal for user {UserId} ({Email})")]
    private partial void LogValidatedPrincipal(long userId, string email);
}
