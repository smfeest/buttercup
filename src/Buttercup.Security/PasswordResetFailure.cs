namespace Buttercup.Security;

/// <summary>
/// Specifies the cause of a password reset failure.
/// </summary>
public enum PasswordResetFailure
{
    /// <summary>
    /// Indicates that password reset token is invalid.
    /// </summary>
    InvalidToken,

    /// <summary>
    /// Indicates that user is deactivated.
    /// </summary>
    UserDeactivated,
}
