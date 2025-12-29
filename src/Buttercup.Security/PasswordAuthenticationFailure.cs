namespace Buttercup.Security;

/// <summary>
/// Defines the causes for password authentication failures.
/// </summary>
public enum PasswordAuthenticationFailure
{
    /// <summary>
    /// Indicates that the email address does not match active user, the password is incorrect, or
    /// the user has yet to set a password.
    /// </summary>
    IncorrectCredentials,

    /// <summary>
    /// Indicates that the request has been blocked due to too many failed attempts. The user must
    /// either reset their password or wait a while before trying again.
    /// </summary>
    TooManyAttempts,
}
