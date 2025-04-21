namespace Buttercup.Security;

/// <summary>
/// Defines the causes for password authentication failures.
/// </summary>
public enum PasswordAuthenticationFailure
{
    /// <summary>
    /// Indicates that the email was unrecognised, the password was incorrect, or the user has yet
    /// to set a password.
    /// </summary>
    IncorrectCredentials,
}
