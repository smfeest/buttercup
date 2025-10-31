using System.ComponentModel.DataAnnotations;

namespace Buttercup.Application;

/// <summary>
/// Represents a new user's attributes.
/// </summary>
public sealed record NewUserAttributes
{
    /// <summary>
    /// Gets or sets the user's name.
    /// </summary>
    /// <value>
    /// The user's name.
    /// </value>
    [Required(ErrorMessage = "Error_RequiredField")]
    [StringLength(250, ErrorMessage = "Error_TooManyCharacters")]
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    /// <value>
    /// The user's email address.
    /// </value>
    [Required(ErrorMessage = "Error_RequiredField")]
    [StringLength(250, ErrorMessage = "Error_TooManyCharacters")]
    [EmailAddress(ErrorMessage = "Error_InvalidEmail")]
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the user's time zone.
    /// </summary>
    /// <value>
    /// The user's time zone as a TZID (e.g. 'Europe/London').
    /// </value>
    [Required(ErrorMessage = "Error_RequiredField")]
    [StringLength(50, ErrorMessage = "Error_TooManyCharacters")]
    public string TimeZone { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the user is an administrator.
    /// </summary>
    /// <value>
    /// <b>true</b> if the user is an administrator, <b>false</b> otherwise.
    /// </value>
    public bool IsAdmin { get; set; }
}
