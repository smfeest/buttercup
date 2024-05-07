using System.ComponentModel.DataAnnotations;

namespace Buttercup.Application.Validation;

/// <summary>
/// Represents a validation error.
/// </summary>
/// <param name="Message">
/// The message.
/// </param>
/// <param name="Instance">
/// The object that failed validation.
/// </param>
/// <param name="Member">
/// The name of the property that failed validation, or null if validation failed at object level.
/// </param>
/// <param name="Value">
/// The value that failed validation.
/// </param>
/// <param name="ValidationAttribute">
/// The validation attribute that raised the error.
/// </param>
public sealed record ValidationError(
    string Message,
    object Instance,
    string? Member,
    object? Value,
    ValidationAttribute ValidationAttribute);
