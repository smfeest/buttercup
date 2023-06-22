using System.Globalization;
using System.Security.Claims;

namespace Buttercup.Security;

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
    /// The user ID.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Principal does not have a name identifier claim.
    /// </exception>
    public static long GetUserId(this ClaimsPrincipal principal) => principal.TryGetUserId() ??
        throw new InvalidOperationException("Principal has no name identifier claim");

    /// <summary>
    /// Gets the user ID stored in the principal's first <see cref="ClaimTypes.NameIdentifier"/>
    /// claim, if the principal has any such claim.
    /// </summary>
    /// <param name="principal">
    /// The principal.
    /// </param>
    /// <returns>
    /// The user ID, or null if the principal does not have a <see
    /// cref="ClaimTypes.NameIdentifier"/> claim.
    /// </returns>
    public static long? TryGetUserId(this ClaimsPrincipal principal)
    {
        var claimValue = principal.FindFirstValue(ClaimTypes.NameIdentifier);

        return claimValue == null ? null : long.Parse(claimValue, CultureInfo.InvariantCulture);
    }
}
