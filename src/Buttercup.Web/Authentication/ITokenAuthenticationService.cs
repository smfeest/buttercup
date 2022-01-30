using Buttercup.Models;

namespace Buttercup.Web.Authentication;

/// <summary>
/// Defines the contract for the token authentication service.
/// </summary>
public interface ITokenAuthenticationService
{
    /// <summary>
    /// Issues an access token.
    /// </summary>
    /// <param name="user">
    /// The user.
    /// </param>
    /// <returns>
    /// A task for the operation. The result is the access token.
    /// </returns>
    Task<string> IssueAccessToken(User user);
}
