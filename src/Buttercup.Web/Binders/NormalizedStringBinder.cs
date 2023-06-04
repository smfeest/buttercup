
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Buttercup.Web.Binders;

/// <summary>
/// A model binder that trims all strings and converts empty strings to null.
/// </summary>
public sealed class NormalizedStringBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var modelName = bindingContext.ModelName;

        var valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        var modelState = bindingContext.ModelState;
        modelState.SetModelValue(modelName, valueProviderResult);

        var trimmedValue = valueProviderResult.FirstValue?.Trim();

        bindingContext.Result = ModelBindingResult.Success(
            string.IsNullOrEmpty(trimmedValue) ? null : trimmedValue);

        return Task.CompletedTask;
    }
}
