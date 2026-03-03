using System.ComponentModel.DataAnnotations;

namespace Buttercup.Application.Validation;

/// <summary>
/// Specifies that the value of a data field must be a valid time zone identifier (TZID).
/// </summary>
[AttributeUsage(
    AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
    AllowMultiple = false)]
public sealed class TimeZoneAttribute : ValidationAttribute
{
    /// <inheritdoc />
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        => value switch
        {
            null => ValidationResult.Success,
            string timeZone =>
                TimeZoneInfo.TryFindSystemTimeZoneById(timeZone, out var timeZoneInfo) &&
                    timeZoneInfo.HasIanaId ?
                    ValidationResult.Success :
                    new ValidationResult($"{timeZone} is not a recognized IANA time zone identifier"),
            _ => new ValidationResult("Time zone identifier must be a string"),
        };
}
