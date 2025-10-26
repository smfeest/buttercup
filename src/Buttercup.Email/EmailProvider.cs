namespace Buttercup.Email;

/// <summary>
/// Specifies how emails are sent.
/// </summary>
public enum EmailProvider
{
    /// <summary>
    /// Indicates that emails are sent via Azure Communication Services.
    /// </summary>
    Azure,

    /// <summary>
    /// Indicates that emails are sent to Mailpit.
    /// </summary>
    Mailpit,
}
