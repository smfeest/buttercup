using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Localization;

namespace Buttercup.Application.Validation;

internal sealed class ValidationErrorLocalizer<T>(IStringLocalizer<T> stringLocalizer)
    : IValidationErrorLocalizer<T>
{
    private readonly IStringLocalizer<T> stringLocalizer = stringLocalizer;

    public string FormatMessage(
        ValidationContext validationContext, ValidationAttribute attribute) =>
        string.IsNullOrEmpty(attribute.ErrorMessage) ?
            attribute.FormatErrorMessage(validationContext.DisplayName) :
            this.stringLocalizer.GetString(
                attribute.ErrorMessage,
                [validationContext.DisplayName, .. GetMessageArguments(attribute)]);

    private static object[] GetMessageArguments(ValidationAttribute attribute) =>
        attribute switch
        {
            RangeAttribute range => [range.Minimum, range.Maximum],
            StringLengthAttribute stringLength =>
                [stringLength.MaximumLength, stringLength.MinimumLength],
            _ => []
        };
}
