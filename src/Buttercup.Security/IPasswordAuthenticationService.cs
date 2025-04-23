using System.Net;
using Buttercup.EntityModel;
using Microsoft.AspNetCore.Mvc;

namespace Buttercup.Security;

/// <summary>
/// Defines the contract for the password authentication service.
/// </summary>
public interface IPasswordAuthenticationService
{
    /// <summary>
    /// Authenticates a user with an email address and password.
    /// </summary>
    /// <param name="email">
    /// The email address.
    /// </param>
    /// <param name="password">
    /// The password.
    /// </param>
    /// <param name="ipAddress">
    /// The client IP address.
    /// </param>
    /// <returns>
    /// A task for the operation.
    /// </returns>
    Task<PasswordAuthenticationResult> Authenticate(string email, string password, IPAddress? ipAddress);

    /// <summary>
    /// Changes a user's password.
    /// </summary>
    /// <param name="userId">
    /// The user ID.
    /// </param>
    /// <param name="currentPassword">
    /// The current password for verification.
    /// </param>
    /// <param name="newPassword">
    /// The new password.
    /// </param>
    /// <param name="ipAddress">
    /// The client IP address.
    /// </param>
    /// <returns>
    /// A task for the operation. The result is <b>true</b> if the password was changed
    /// successfully, or <b>false</b> if the current password was incorrect.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// The user doesn't have a password.
    /// </exception>
    Task<bool> ChangePassword(
        long userId, string currentPassword, string newPassword, IPAddress? ipAddress);

    /// <summary>
    /// Validates a password reset token.
    /// </summary>
    /// <param name="token">
    /// The password reset token.
    /// </param>
    /// <param name="ipAddress">
    /// The client IP address.
    /// </param>
    /// <returns>
    /// A task for the operation. The result is <b>true</b> if the token is valid, <b>false</b>
    /// if it isn't.
    /// </returns>
    Task<bool> PasswordResetTokenIsValid(string token, IPAddress? ipAddress);

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
    /// <param name="ipAddress">
    /// The client IP address.
    /// </param>
    /// <returns>
    /// A task for the operation. The result is the updated user.
    /// </returns>
    /// <exception cref="InvalidTokenException">
    /// The password reset token isn't valid.
    /// </exception>
    Task<User> ResetPassword(string token, string newPassword, IPAddress? ipAddress);

    /// <summary>
    /// Sends a password reset link to the user with a given email address.
    /// </summary>
    /// <remarks>
    /// No email is sent if there is no user with the specified email address.
    /// </remarks>
    /// <param name="email">
    /// The email address.
    /// </param>
    /// <param name="ipAddress">
    /// The client IP address.
    /// </param>
    /// <param name="urlHelper">
    /// The URL helper.
    /// </param>
    /// <returns>
    /// A task for the operation.
    /// </returns>
    Task SendPasswordResetLink(string email, IPAddress? ipAddress, IUrlHelper urlHelper);
}
