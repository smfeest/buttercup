using Xunit;

namespace Buttercup.EntityModel;

public sealed class CommentActionToStringConverterTests
{
    [Theory]
    [InlineData(CommentAction.Create, "create")]
    [InlineData(CommentAction.Delete, "delete")]
    public void ConvertsToAndFromExpectedString(CommentAction action, string stringValue)
    {
        var converter = new CommentActionToStringConverter();
        Assert.Equal(stringValue, converter.ConvertToProviderTyped(action));
        Assert.Equal(action, converter.ConvertFromProviderTyped(stringValue));
    }

    [Fact]
    public void ThrowsWhenInvalidStringProvided()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new CommentActionToStringConverter().ConvertFromProviderTyped("foo"));
        Assert.Contains("Invalid action 'foo'", exception.Message);
    }

    [Fact]
    public void ThrowsWhenInvalidEnumValueProvided()
    {
        var invalidValue = (CommentAction)999;
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new CommentActionToStringConverter().ConvertToProviderTyped(invalidValue));
        Assert.Equal(invalidValue, exception.ActualValue);
    }
}
