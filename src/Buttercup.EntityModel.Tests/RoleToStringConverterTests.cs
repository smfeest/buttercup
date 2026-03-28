using Xunit;

namespace Buttercup.EntityModel;

public sealed class RoleToStringConverterTests
{
    [Theory]
    [InlineData(Role.Admin, "admin")]
    [InlineData(Role.Contributor, "contributor")]
    public void ConvertsToAndFromExpectedString(Role role, string stringValue)
    {
        var converter = new RoleToStringConverter();
        Assert.Equal(stringValue, converter.ConvertToProviderTyped(role));
        Assert.Equal(role, converter.ConvertFromProviderTyped(stringValue));
    }

    [Fact]
    public void ThrowsWhenInvalidStringProvided()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new RoleToStringConverter().ConvertFromProviderTyped("foo"));
        Assert.Contains("Invalid role 'foo'", exception.Message);
    }

    [Fact]
    public void ThrowsWhenInvalidEnumValueProvided()
    {
        var invalidValue = (Role)999;
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new RoleToStringConverter().ConvertToProviderTyped(invalidValue));
        Assert.Equal(invalidValue, exception.ActualValue);
    }
}
