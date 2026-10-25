using System.ComponentModel.DataAnnotations;

namespace Buttercup.Application.Validation;

/// <summary>
/// Defines the contract for the validation error localizer.
/// </summary>
/// <typeparam name="T">
/// The type of object being validated.
/// </typeparam>
internal interface IValidationErrorLocalizer<T>
{
    /// <summary>
    /// Formats a localized validation error message.
    /// </summary>
    /// <param name="validationContext">
    /// The validation context.
    /// </param>
    /// <param name="attribute">
    /// The validation attribute.
    /// </param>
    /// <returns>
    /// The localized validation error message.
    /// </returns>
    string FormatMessage(ValidationContext validationContext, ValidationAttribute attribute);
}
