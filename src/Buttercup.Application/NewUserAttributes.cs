using System.ComponentModel.DataAnnotations;
using Buttercup.Application.Validation;

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
    /// <remarks>
    /// If this property is left unspecified, the new user's time zone will be set to <see
    /// cref="GlobalizationOptions.DefaultUserTimeZone"/>.
    /// </remarks>
    /// <value>
    /// The user's time zone as a TZID (e.g. 'Europe/London').
    /// </value>
    [StringLength(50, ErrorMessage = "Error_TooManyCharacters")]
    [TimeZone(ErrorMessage = "Error_InvalidTimeZone")]
    public string? TimeZone { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether the user is an administrator.
    /// </summary>
    /// <value>
    /// <b>true</b> if the user is an administrator, <b>false</b> otherwise.
    /// </value>
    public bool IsAdmin { get; set; }
}
