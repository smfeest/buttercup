using System.Security.Claims;
using Buttercup.EntityModel;

namespace Buttercup.Security;

/// <summary>
/// Defines the contract for the claims identity factory.
/// </summary>
public interface IClaimsIdentityFactory
{
    /// <summary>
    /// Creates a claims identity representing a user.
    /// </summary>
    /// <param name="user">
    /// The user.
    /// </param>
    /// <param name="authenticationType">
    /// The authentication type.
    /// </param>
    /// <returns>
    /// The claims identity.
    /// </returns>
    ClaimsIdentity CreateIdentityForUser(User user, string authenticationType);
}
