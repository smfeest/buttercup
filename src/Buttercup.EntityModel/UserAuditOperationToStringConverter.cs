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
        "authenticate_password" => UserAuditOperation.AuthenticatePassword,
        "change_password" => UserAuditOperation.ChangePassword,
        "create" => UserAuditOperation.Create,
        "create_password_reset_token" => UserAuditOperation.CreatePasswordResetToken,
        "deactivate" => UserAuditOperation.Deactivate,
        "reactivate" => UserAuditOperation.Reactivate,
        "reset_password" => UserAuditOperation.ResetPassword,
        "sign_in" => UserAuditOperation.SignIn,
        "sign_out" => UserAuditOperation.SignOut,
        _ => throw new ArgumentException($"Invalid user operation type '{value}'", nameof(value))
    };

    private static string ToString(UserAuditOperation operation) => operation switch
    {
        UserAuditOperation.AuthenticatePassword => "authenticate_password",
        UserAuditOperation.ChangePassword => "change_password",
        UserAuditOperation.Create => "create",
        UserAuditOperation.CreatePasswordResetToken => "create_password_reset_token",
        UserAuditOperation.Deactivate => "deactivate",
        UserAuditOperation.Reactivate => "reactivate",
        UserAuditOperation.ResetPassword => "reset_password",
        UserAuditOperation.SignIn => "sign_in",
        UserAuditOperation.SignOut => "sign_out",
        _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null)
    };
}
