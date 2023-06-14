using Buttercup.EntityModel;

namespace Buttercup.Security;

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

    /// <summary>
    /// Validates an access token.
    /// </summary>
    /// <param name="accessToken">
    /// The access token.
    /// </param>
    /// <returns>
    /// A task for the operation. The result is the authenticated user if the token is valid, or
    /// null reference if it isn't.
    /// </returns>
    Task<User?> ValidateAccessToken(string accessToken);
}
