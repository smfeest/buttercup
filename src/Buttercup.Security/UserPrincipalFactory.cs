using System.Globalization;
using System.Security.Claims;
using Buttercup.EntityModel;

namespace Buttercup.Security;

internal sealed class UserPrincipalFactory : IUserPrincipalFactory
{
    public ClaimsPrincipal Create(User user, string authenticationType)
    {
        var claims = new Claim[]
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString(CultureInfo.InvariantCulture)),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email),
            new(CustomClaimTypes.SecurityStamp, user.SecurityStamp),
            new(CustomClaimTypes.TimeZone, user.TimeZone),
        };

        return new(new ClaimsIdentity(claims, authenticationType));
    }
}
