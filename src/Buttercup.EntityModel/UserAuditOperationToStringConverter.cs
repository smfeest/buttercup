using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Buttercup.EntityModel;

internal sealed class UserAuditOperationToStringConverter
    : ValueConverter<UserAuditOperation, string>
{
    public UserAuditOperationToStringConverter() : base(v => ToString(v), v => FromString(v))
    {
    }

    private static UserAuditOperation FromString(string value) => value switch
    {
        "change_password" => UserAuditOperation.ChangePassword,
        "create" => UserAuditOperation.Create,
        "deactivate" => UserAuditOperation.Deactivate,
        "reactivate" => UserAuditOperation.Reactivate,
        "reset_password" => UserAuditOperation.ResetPassword,
        _ => throw new ArgumentException($"Invalid user operation type '{value}'", nameof(value))
    };

    private static string ToString(UserAuditOperation operation) => operation switch
    {
        UserAuditOperation.ChangePassword => "change_password",
        UserAuditOperation.Create => "create",
        UserAuditOperation.Deactivate => "deactivate",
        UserAuditOperation.Reactivate => "reactivate",
        UserAuditOperation.ResetPassword => "reset_password",
        _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
    };
}
