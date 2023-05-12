using System.Globalization;
using System.Security.Claims;
using Buttercup.EntityModel;

namespace Buttercup.Web.Authentication;

public sealed class UserPrincipalFactory : IUserPrincipalFactory
{
    public ClaimsPrincipal Create(User user, string authenticationType)
    {
        var claims = new Claim[]
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString(CultureInfo.InvariantCulture)),
            new(ClaimTypes.Email, user.Email),
            new(CustomClaimTypes.SecurityStamp, user.SecurityStamp),
        };

        return new(new ClaimsIdentity(claims, authenticationType, ClaimTypes.Email, null));
    }
}
