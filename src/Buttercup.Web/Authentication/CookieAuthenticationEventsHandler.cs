using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Buttercup.Web.Authentication
{
    public class CookieAuthenticationEventsHandler : CookieAuthenticationEvents
    {
        public CookieAuthenticationEventsHandler(IAuthenticationManager authenticationManager) =>
            this.AuthenticationManager = authenticationManager;

        public IAuthenticationManager AuthenticationManager { get; }

        public override Task ValidatePrincipal(CookieValidatePrincipalContext context) =>
            this.AuthenticationManager.ValidatePrincipal(context);
    }
}
