using System.Globalization;
using System.Security.Claims;
using Buttercup.DataAccess;
using Buttercup.EntityModel;
using Buttercup.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace Buttercup.Web.Authentication;

public sealed class CookieAuthenticationEventsHandler : CookieAuthenticationEvents
{
    private readonly IAuthenticationService authenticationService;
    private readonly IDbContextFactory<AppDbContext> dbContextFactory;
    private readonly ILogger<CookieAuthenticationEventsHandler> logger;
    private readonly IUserDataProvider userDataProvider;
    private readonly IUserPrincipalFactory userPrincipalFactory;

    public CookieAuthenticationEventsHandler(
        IAuthenticationService authenticationService,
        IDbContextFactory<AppDbContext> dbContextFactory,
        ILogger<CookieAuthenticationEventsHandler> logger,
        IUserDataProvider userDataProvider,
        IUserPrincipalFactory userPrincipalFactory)
    {
        this.authenticationService = authenticationService;
        this.dbContextFactory = dbContextFactory;
        this.logger = logger;
        this.userDataProvider = userDataProvider;
        this.userPrincipalFactory = userPrincipalFactory;
    }

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

        using (var dbContext = this.dbContextFactory.CreateDbContext())
        {
            user = await this.userDataProvider.GetUser(dbContext, userId.Value);
        }

        var securityStamp = principal.FindFirstValue(CustomClaimTypes.SecurityStamp);

        if (!string.Equals(securityStamp, user.SecurityStamp, StringComparison.Ordinal))
        {
            ValidatePrincipalLogMessages.IncorrectSecurityStamp(
                this.logger, user.Id, user.Email, null);

            context.RejectPrincipal();

            await this.authenticationService.SignOutAsync(
                context.HttpContext, context.Scheme.Name, null);

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
    }
}
