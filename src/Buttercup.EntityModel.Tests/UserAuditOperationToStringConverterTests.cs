using Xunit;

namespace Buttercup.EntityModel;

public sealed class UserAuditOperationToStringConverterTests
{
    [Theory]
    [InlineData(UserAuditOperation.AuthenticatePassword, "authenticate_password")]
    [InlineData(UserAuditOperation.ChangePassword, "change_password")]
    [InlineData(UserAuditOperation.Create, "create")]
    [InlineData(UserAuditOperation.CreatePasswordResetToken, "create_password_reset_token")]
    [InlineData(UserAuditOperation.Deactivate, "deactivate")]
    [InlineData(UserAuditOperation.Reactivate, "reactivate")]
    [InlineData(UserAuditOperation.ResetPassword, "reset_password")]
    public void ConvertsToAndFromExpectedString(UserAuditOperation operation, string stringValue)
    {
        var converter = new UserAuditOperationToStringConverter();
        Assert.Equal(stringValue, converter.ConvertToProviderTyped(operation));
        Assert.Equal(operation, converter.ConvertFromProviderTyped(stringValue));
    }

    [Fact]
    public void ThrowsWhenInvalidStringProvided()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new UserAuditOperationToStringConverter().ConvertFromProviderTyped("foo"));
        Assert.Contains("Invalid user operation type 'foo'", exception.Message);
    }

    [Fact]
    public void ThrowsWhenInvalidEnumValueProvided()
    {
        var invalidValue = (UserAuditOperation)999;
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new UserAuditOperationToStringConverter().ConvertToProviderTyped(invalidValue));
        Assert.Equal(invalidValue, exception.ActualValue);
    }
}
