using Xunit;

namespace Buttercup.EntityModel;

public sealed class UserOperationToStringConverterTests
{
    [Theory]
    [InlineData(UserOperation.ChangePassword, "change_password")]
    [InlineData(UserOperation.Create, "create")]
    [InlineData(UserOperation.ResetPassword, "reset_password")]
    public void ConvertsToAndFromExpectedString(UserOperation operation, string stringValue)
    {
        var converter = new UserOperationToStringConverter();
        Assert.Equal(stringValue, converter.ConvertToProviderTyped(operation));
        Assert.Equal(operation, converter.ConvertFromProviderTyped(stringValue));
    }

    [Fact]
    public void ThrowsWhenInvalidStringProvided()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new UserOperationToStringConverter().ConvertFromProviderTyped("foo"));
        Assert.Contains("Invalid user operation type 'foo'", exception.Message);
    }

    [Fact]
    public void ThrowsWhenInvalidEnumValueProvided()
    {
        var invalidValue = (UserOperation)999;
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new UserOperationToStringConverter().ConvertToProviderTyped(invalidValue));
        Assert.Equal(invalidValue, exception.ActualValue);
    }
}
