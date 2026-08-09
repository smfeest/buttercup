using Xunit;

namespace Buttercup.EntityModel;

public sealed class RecipeActionToStringConverterTests
{
    [Theory]
    [InlineData(RecipeAction.Create, "create")]
    [InlineData(RecipeAction.Delete, "delete")]
    [InlineData(RecipeAction.Update, "update")]
    public void ConvertsToAndFromExpectedString(RecipeAction action, string stringValue)
    {
        var converter = new RecipeActionToStringConverter();
        Assert.Equal(stringValue, converter.ConvertToProviderTyped(action));
        Assert.Equal(action, converter.ConvertFromProviderTyped(stringValue));
    }

    [Fact]
    public void ThrowsWhenInvalidStringProvided()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new RecipeActionToStringConverter().ConvertFromProviderTyped("foo"));
        Assert.Contains("Invalid action 'foo'", exception.Message);
    }

    [Fact]
    public void ThrowsWhenInvalidEnumValueProvided()
    {
        var invalidValue = (RecipeAction)999;
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new RecipeActionToStringConverter().ConvertToProviderTyped(invalidValue));
        Assert.Equal(invalidValue, exception.ActualValue);
    }
}
