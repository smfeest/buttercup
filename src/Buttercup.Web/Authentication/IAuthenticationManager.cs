using Buttercup.EntityModel;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace Buttercup.Web.Authentication;

/// <summary>
/// Defines the contract for the authentication manager.
/// </summary>
public interface IAuthenticationManager
{
    /// <summary>
    /// Authenticate a user with an email address and password.
    /// </summary>
    /// <param name="email">
    /// The email address.
    /// </param>
    /// <param name="password">
    /// The password.
    /// </param>
    /// <returns>
    /// A task for the operation. The result is the user if successfully authenticated, or a null
    /// reference otherwise.
    /// </returns>
    Task<User?> Authenticate(string email, string password);

    /// <summary>
    /// Changes a user's password.
    /// </summary>
    /// <param name="httpContext">
    /// The HTTP context for the current request.
    /// </param>
    /// <param name="currentPassword">
    /// The current password for verification.
    /// </param>
    /// <param name="newPassword">
    /// The new password.
    /// </param>
    /// <returns>
    /// A task for the operation. The result is <b>true</b> if the password was changed
    /// successfully, or <b>false</b> if the current password was incorrect.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// The user doesn't have a password.
    /// </exception>
    Task<bool> ChangePassword(
        HttpContext httpContext, string currentPassword, string newPassword);

    /// <summary>
    /// Validates a password reset token.
    /// </summary>
    /// <param name="token">
    /// The password reset token.
    /// </param>
    /// <returns>
    /// A task for the operation. The result is <b>true</b> if the token is valid, <b>false</b>
    /// if it isn't.
    /// </returns>
    Task<bool> PasswordResetTokenIsValid(string token);

    /// <summary>
    /// Resets a user's password.
    /// </summary>
    /// <remarks>
    /// All existing password reset tokens for the user are invalidated.
    /// </remarks>
    /// <param name="token">
    /// The password reset token.
    /// </param>
    /// <param name="newPassword">
    /// The new password.
    /// </param>
    /// <returns>
    /// A task for the operation. The result is the updated user.
    /// </returns>
    /// <exception cref="InvalidTokenException">
    /// The password reset token isn't valid.
    /// </exception>
    Task<User> ResetPassword(string token, string newPassword);

    /// <summary>
    /// Sends a password reset link to the user with a given email address.
    /// </summary>
    /// <remarks>
    /// No email is sent if there is no user with the specified email address.
    /// </remarks>
    /// <param name="actionContext">
    /// The current action context.
    /// </param>
    /// <param name="email">
    /// The email address.
    /// </param>
    /// <returns>
    /// A task for the operation.
    /// </returns>
    Task SendPasswordResetLink(ActionContext actionContext, string email);

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

    /// <summary>
    /// Validates a principal from an authentication cookie.
    /// </summary>
    /// <param name="context">
    /// The validation context.
    /// </param>
    /// <returns>
    /// A task for the operation.
    /// </returns>
    Task ValidatePrincipal(CookieValidatePrincipalContext context);
}
