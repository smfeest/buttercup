using System.Globalization;
using System.Security.Claims;

namespace Buttercup.Web.Authentication;

/// <summary>
/// Provides extension methods for <see cref="ClaimsPrincipal" />.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Gets the user ID stored in the principal's first name identifier claim.
    /// </summary>
    /// <param name="principal">
    /// The principal.
    /// </param>
    /// <returns>
    /// The user ID, or null if the principal does not have a name identifier claim.
    /// </returns>
    public static long? GetUserId(this ClaimsPrincipal principal)
    {
        var claimValue = principal.FindFirstValue(ClaimTypes.NameIdentifier);

        return claimValue == null ? null : long.Parse(claimValue, CultureInfo.InvariantCulture);
    }
}
