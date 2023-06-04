
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Buttercup.Web.Binders;

/// <summary>
/// A binding provider for binding string values with normalization.
/// </summary>
public sealed class NormalizedStringBinderProvider : IModelBinderProvider
{
    public IModelBinder? GetBinder(ModelBinderProviderContext context) =>
        context.Metadata.ModelType == typeof(string) ? new NormalizedStringBinder() : null;
}
