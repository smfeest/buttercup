namespace Buttercup.Security;

/// <summary>
/// Defines the contract for the authentication mailer.
/// </summary>
public interface IAuthenticationMailer
{
    /// <summary>
    /// Sends a password change notification.
    /// </summary>
    /// <param name="email">
    /// The recipient's email address.
    /// </param>
    /// <returns>
    /// A task for the operation.
    /// </returns>
    Task SendPasswordChangeNotification(string email);

    /// <summary>
    /// Sends a password reset link.
    /// </summary>
    /// <param name="email">
    /// The recipient's email address.
    /// </param>
    /// <param name="link">
    /// The password reset link.
    /// </param>
    /// <returns>
    /// A task for the operation.
    /// </returns>
    Task SendPasswordResetLink(string email, string link);
}
