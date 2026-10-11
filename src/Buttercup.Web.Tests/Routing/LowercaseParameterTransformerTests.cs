using Xunit;

namespace Buttercup.Web.Routing;

public sealed class LowercaseParameterTransformerTests
{
    #region TransformOutbound

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData(5, "5")]
    [InlineData("FooBar", "foobar")]
    public void TransformOutbound_ConvertsValueToLowerCase(
        object? routeValue, string? expectedResult) =>
        Assert.Equal(
            expectedResult, new LowercaseParameterTransformer().TransformOutbound(routeValue));

    #endregion
}
