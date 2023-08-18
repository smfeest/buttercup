using Buttercup.EntityModel;
using Microsoft.AspNetCore.Http;

namespace Buttercup.Security;

/// <summary>
/// Defines the contract for the cookie authentication service.
/// </summary>
public interface ICookieAuthenticationService
{
    /// <summary>
    /// Refreshes the claims principal for the signed in user, if any, based on the latest
    /// attributes in the database.
    /// </summary>
    /// <param name="httpContext">
    /// The HTTP context for the current request.
    /// </param>
    /// <returns>
    /// A task for the operation. The result is <b>true</b> if there was a claims principal to
    /// refresh, <b>false</b> otherwise.
    /// </returns>
    Task<bool> RefreshPrincipal(HttpContext httpContext);

    /// <summary>
    /// Signs in a user.
    /// </summary>
    /// <param name="httpContext">
    /// The HTTP context for the current request.
    /// </param>
    /// <param name="user">
    /// The user.
    /// </param>
    /// <returns>
    /// A task for the operation.
    /// </returns>
    Task SignIn(HttpContext httpContext, User user);

    /// <summary>
    /// Signs out the current user.
    /// </summary>
    /// <param name="httpContext">
    /// The HTTP context for the current request.
    /// </param>
    /// <returns>
    /// A task for the operation.
    /// </returns>
    Task SignOut(HttpContext httpContext);
}
