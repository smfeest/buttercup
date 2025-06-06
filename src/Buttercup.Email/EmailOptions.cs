using System.ComponentModel.DataAnnotations;

namespace Buttercup.Email;

/// <summary>
/// The email options.
/// </summary>
public sealed class EmailOptions
{
    /// <summary>
    /// Gets or sets the from address.
    /// </summary>
    /// <value>
    /// The from address.
    /// </value>
    [Required]
    public required string FromAddress { get; set; }
}
