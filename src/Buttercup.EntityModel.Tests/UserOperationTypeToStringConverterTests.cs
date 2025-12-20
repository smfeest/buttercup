using Xunit;

namespace Buttercup.EntityModel;

public sealed class UserOperationTypeToStringConverterTests
{
    [Theory]
    [InlineData(UserOperationType.ChangePassword, "change_password")]
    [InlineData(UserOperationType.Create, "create")]
    [InlineData(UserOperationType.ResetPassword, "reset_password")]
    public void ConvertsToAndFromExpectedString(UserOperationType operationType, string stringValue)
    {
        var converter = new UserOperationTypeToStringConverter();
        Assert.Equal(stringValue, converter.ConvertToProviderTyped(operationType));
        Assert.Equal(operationType, converter.ConvertFromProviderTyped(stringValue));
    }

    [Fact]
    public void ThrowsWhenInvalidStringProvided()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new UserOperationTypeToStringConverter().ConvertFromProviderTyped("foo"));
        Assert.Contains("Invalid user operation type 'foo'", exception.Message);
    }

    [Fact]
    public void ThrowsWhenInvalidEnumValueProvided()
    {
        var invalidValue = (UserOperationType)999;
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new UserOperationTypeToStringConverter().ConvertToProviderTyped(invalidValue));
        Assert.Equal(invalidValue, exception.ActualValue);
    }
}
