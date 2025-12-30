using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Buttercup.EntityModel;

internal sealed class UserOperationToStringConverter : ValueConverter<UserOperation, string>
{
    public UserOperationToStringConverter() : base(v => ToString(v), v => FromString(v))
    {
    }

    private static UserOperation FromString(string value) => value switch
    {
        "change_password" => UserOperation.ChangePassword,
        "create" => UserOperation.Create,
        "deactivate" => UserOperation.Deactivate,
        "reset_password" => UserOperation.ResetPassword,
        _ => throw new ArgumentException($"Invalid user operation type '{value}'", nameof(value))
    };

    private static string ToString(UserOperation operation) => operation switch
    {
        UserOperation.ChangePassword => "change_password",
        UserOperation.Create => "create",
        UserOperation.Deactivate => "deactivate",
        UserOperation.ResetPassword => "reset_password",
        _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
    };
}
