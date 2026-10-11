namespace Buttercup.Web.Routing;

public sealed class LowercaseParameterTransformer : IOutboundParameterTransformer
{
    public string? TransformOutbound(object? value) => value?.ToString()?.ToLowerInvariant();
}
