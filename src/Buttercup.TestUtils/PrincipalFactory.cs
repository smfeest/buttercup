using System.Globalization;
using System.Security.Claims;

namespace Buttercup.TestUtils;

/// <summary>
/// Provides methods for creating <see cref="ClaimsPrincipal"/> instances in tests.
/// </summary>
public static class PrincipalFactory
{
    /// <summary>
    /// Creates a <see cref="ClaimsPrincipal"/> with a <see cref="ClaimTypes.NameIdentifier"/> claim
    /// representing the user ID.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="additionalClaims">Any additional claims to include.</param>
    /// <returns>The principal.</returns>
    public static ClaimsPrincipal CreateWithUserId(long userId, params Claim[] additionalClaims)
    {
        var identity = new ClaimsIdentity();

        identity.AddClaim(
            new Claim(ClaimTypes.NameIdentifier, userId.ToString(CultureInfo.InvariantCulture)));
        identity.AddClaims(additionalClaims);

        return new(identity);
    }
}
