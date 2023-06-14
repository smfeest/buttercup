using Buttercup.Security;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Buttercup.Web.Authentication;

public sealed class CookieAuthenticationEventsHandler : CookieAuthenticationEvents
{
    private readonly IAuthenticationManager authenticationManager;

    public CookieAuthenticationEventsHandler(IAuthenticationManager authenticationManager) =>
        this.authenticationManager = authenticationManager;

    public override Task ValidatePrincipal(CookieValidatePrincipalContext context) =>
        this.authenticationManager.ValidatePrincipal(context);
}
