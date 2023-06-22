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
    /// <returns>The principal.</returns>
    public static ClaimsPrincipal CreateWithUserId(long userId)
    {
        var idClaim = new Claim(
            ClaimTypes.NameIdentifier,
            userId.ToString(CultureInfo.InvariantCulture));

        var identity = new ClaimsIdentity(new[] { idClaim });

        return new(identity);
    }
}
