using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.Extensions.Localization;

namespace Buttercup.Application.Validation;

/// <summary>
/// A validation attribute adapter provider that falls back to <see cref="GenericAttributeAdapter"/>
/// for validation attributes that do not have a dedicated adapter.
/// </summary>
public class GenericValidationAttributeAdapterProvider : IValidationAttributeAdapterProvider
{
    private readonly ValidationAttributeAdapterProvider baseProvider = new();

    /// <inheritdoc/>
    public IAttributeAdapter? GetAttributeAdapter(
        ValidationAttribute attribute, IStringLocalizer? stringLocalizer) =>
        this.baseProvider.GetAttributeAdapter(attribute, stringLocalizer) ??
        new GenericAttributeAdapter(attribute, stringLocalizer);
}
