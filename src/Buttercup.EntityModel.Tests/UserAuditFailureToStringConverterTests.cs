using Xunit;

namespace Buttercup.EntityModel;

public sealed class UserAuditFailureToStringConverterTests
{
    [Theory]
    [InlineData(UserAuditFailure.IncorrectPassword, "incorrect_password")]
    [InlineData(UserAuditFailure.NoPasswordSet, "no_password_set")]
    [InlineData(UserAuditFailure.UserDeactivated, "user_deactivated")]
    public void ConvertsToAndFromExpectedString(UserAuditFailure failure, string stringValue)
    {
        var converter = new UserAuditFailureToStringConverter();
        Assert.Equal(stringValue, converter.ConvertToProviderTyped(failure));
        Assert.Equal(failure, converter.ConvertFromProviderTyped(stringValue));
    }

    [Fact]
    public void ThrowsWhenInvalidStringProvided()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new UserAuditFailureToStringConverter().ConvertFromProviderTyped("foo"));
        Assert.Contains("Invalid user audit failure 'foo'", exception.Message);
    }

    [Fact]
    public void ThrowsWhenInvalidEnumValueProvided()
    {
        var invalidValue = (UserAuditFailure)999;
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new UserAuditFailureToStringConverter().ConvertToProviderTyped(invalidValue));
        Assert.Equal(invalidValue, exception.ActualValue);
    }
}
