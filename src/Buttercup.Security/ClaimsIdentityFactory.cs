using System.Globalization;
using System.Security.Claims;
using Buttercup.EntityModel;

namespace Buttercup.Security;

internal sealed class ClaimsIdentityFactory : IClaimsIdentityFactory
{
    public ClaimsIdentity CreateIdentityForUser(User user, string authenticationType)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString(CultureInfo.InvariantCulture)),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email),
            new(CustomClaimTypes.SecurityStamp, user.SecurityStamp),
            new(CustomClaimTypes.TimeZone, user.TimeZone),
            new(CustomClaimTypes.UserRevision,
                user.Revision.ToString(CultureInfo.InvariantCulture)),
        };

        if (user.IsAdmin)
        {
            claims.Add(new(ClaimTypes.Role, RoleNames.Admin));
        }

        return new(claims, authenticationType);
    }
}
