using System.Globalization;
using System.Security.Claims;
using Buttercup.EntityModel;

namespace Buttercup.Security;

internal sealed class ClaimsIdentityFactory : IClaimsIdentityFactory
{
    public ClaimsIdentity CreateIdentityForUser(User user, string? authenticationType)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString(CultureInfo.InvariantCulture)),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
            new(CustomClaimTypes.SecurityStamp, user.SecurityStamp),
            new(CustomClaimTypes.TimeZone, user.TimeZone),
            new(CustomClaimTypes.UserRevision,
                user.Revision.ToString(CultureInfo.InvariantCulture)),
        };

        return new(claims, authenticationType);
    }
}
