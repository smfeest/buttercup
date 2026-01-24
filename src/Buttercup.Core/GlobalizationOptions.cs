using System.ComponentModel.DataAnnotations;

namespace Buttercup;

/// <summary>
/// The globalization options.
/// </summary>
public sealed record GlobalizationOptions
{
    /// <summary>
    /// Gets or sets the TZID of the default time zone for new users.
    /// </summary>
    /// <value>
    /// The TZID of the default time zone for new users.
    /// </value>
    [Required]
    public required string DefaultUserTimeZone { get; set; }
}
