namespace Buttercup.Email;

/// <summary>
/// The email options.
/// </summary>
public class EmailOptions
{
    /// <summary>
    /// Gets or sets the SendGrid API key.
    /// </summary>
    /// <value>
    /// The SendGrid API key.
    /// </value>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Gets or sets the from address.
    /// </summary>
    /// <value>
    /// The from address.
    /// </value>
    public string? FromAddress { get; set; }
}
