using System.Security.Claims;
using Buttercup.EntityModel;

namespace Buttercup.Web.Authentication;

/// <summary>
/// Defines the contract for the user principal factory.
/// </summary>
public interface IUserPrincipalFactory
{
    /// <summary>
    /// Creates a claims principal representing a user.
    /// </summary>
    /// <param name="user">
    /// The user.
    /// </param>
    /// <param name="authenticationType">
    /// The authentication type.
    /// </param>
    /// <returns>
    /// The claims principal.
    /// </returns>
    ClaimsPrincipal Create(User user, string authenticationType);
}
