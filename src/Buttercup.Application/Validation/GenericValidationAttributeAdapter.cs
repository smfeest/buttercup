using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.Localization;

namespace Buttercup.Application.Validation;

/// <summary>
/// An adapter that provides basic localization support for any validation attribute.
/// </summary>
public sealed class GenericAttributeAdapter(
    ValidationAttribute attribute, IStringLocalizer? stringLocalizer)
    : AttributeAdapterBase<ValidationAttribute>(attribute, stringLocalizer)
{
    /// <inheritdoc/>
    public override void AddValidation(ClientModelValidationContext context)
    {
    }

    /// <inheritdoc/>
    public override string GetErrorMessage(ModelValidationContextBase validationContext) =>
        this.GetErrorMessage(
            validationContext.ModelMetadata, validationContext.ModelMetadata.GetDisplayName());
}
