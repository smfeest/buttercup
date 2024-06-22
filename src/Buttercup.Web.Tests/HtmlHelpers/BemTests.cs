using Xunit;

namespace Buttercup.Web.HtmlHelpers;

public sealed class BemTests
{
    [Theory]
    [InlineData("foo", new string?[0], "foo")]
    [InlineData("foo", new string?[] { null }, "foo")]
    [InlineData("foo", new string?[] { "bar", null, "", "baz" }, "foo foo--bar foo--baz")]
    public void Block_ReturnsBlockClasses(
        string block, string?[] modifiers, string expectedClasses) =>
        Assert.Equal(expectedClasses, Bem.Block(block, modifiers));
}
