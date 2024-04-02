using System.Globalization;
using System.Security.Claims;
using Buttercup.Application;
using Buttercup.EntityModel;
using Buttercup.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Buttercup.Web.Security;

public sealed class CookieAuthenticationEventsHandler(
    IAuthenticationService authenticationService,
    ILogger<CookieAuthenticationEventsHandler> logger,
    IUserManager userManager,
    IUserPrincipalFactory userPrincipalFactory)
    : CookieAuthenticationEvents
{
    private readonly IAuthenticationService authenticationService = authenticationService;
    private readonly ILogger<CookieAuthenticationEventsHandler> logger = logger;
    private readonly IUserManager userManager = userManager;
    private readonly IUserPrincipalFactory userPrincipalFactory = userPrincipalFactory;

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

        User user;

        try
        {
            user = await this.userManager.GetUser(userId.Value);
        }
        catch (NotFoundException)
        {
            ValidatePrincipalLogMessages.UserNoLongerExists(this.logger, userId.Value, null);
            await this.RejectPrincipalAndSignOut(context);
            return;
        }

        var securityStamp = principal.FindFirstValue(CustomClaimTypes.SecurityStamp);

        if (!string.Equals(securityStamp, user.SecurityStamp, StringComparison.Ordinal))
        {
            ValidatePrincipalLogMessages.IncorrectSecurityStamp(
                this.logger, user.Id, user.Email, null);
            await this.RejectPrincipalAndSignOut(context);
            return;
        }

        ValidatePrincipalLogMessages.Success(this.logger, user.Id, user.Email, null);

        var userRevision = principal.FindFirstValue(CustomClaimTypes.UserRevision);

        if (userRevision is null ||
            int.Parse(userRevision, CultureInfo.InvariantCulture) != user.Revision)
        {
            context.ReplacePrincipal(this.userPrincipalFactory.Create(user, context.Scheme.Name));
            context.ShouldRenew = true;

            ValidatePrincipalLogMessages.RefreshedClaimsPrincipal(
                this.logger, user.Id, user.Email, null);
        }
    }

    private async Task RejectPrincipalAndSignOut(CookieValidatePrincipalContext context)
    {
        context.RejectPrincipal();

        await this.authenticationService.SignOutAsync(
            context.HttpContext, context.Scheme.Name, null);
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

        public static readonly Action<ILogger, long, string, Exception?> RefreshedClaimsPrincipal =
            LoggerMessage.Define<long, string>(
                LogLevel.Information,
                216,
                "Refreshed claims principal for user {UserId} ({Email})");

        public static readonly Action<ILogger, long, Exception?> UserNoLongerExists =
            LoggerMessage.Define<long>(LogLevel.Information, 219, "User {UserId} no longer exists");
    }
}
