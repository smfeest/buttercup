using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Buttercup.EntityModel;

internal sealed class UserOperationTypeToStringConverter : ValueConverter<UserOperationType, string>
{
    public UserOperationTypeToStringConverter() : base(v => ToString(v), v => FromString(v))
    {
    }

    private static UserOperationType FromString(string value) => value switch
    {
        "change_password" => UserOperationType.ChangePassword,
        "create" => UserOperationType.Create,
        "reset_password" => UserOperationType.ResetPassword,
        _ => throw new ArgumentException($"Invalid user operation type '{value}'", nameof(value))
    };

    private static string ToString(UserOperationType operationType) => operationType switch
    {
        UserOperationType.ChangePassword => "change_password",
        UserOperationType.Create => "create",
        UserOperationType.ResetPassword => "reset_password",
        _ => throw new ArgumentOutOfRangeException(nameof(operationType), operationType, null)
    };
}
