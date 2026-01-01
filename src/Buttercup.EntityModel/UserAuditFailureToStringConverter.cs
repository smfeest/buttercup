using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Buttercup.EntityModel;

internal sealed class UserAuditFailureToStringConverter : ValueConverter<UserAuditFailure, string>
{
    public UserAuditFailureToStringConverter() : base(v => ToString(v), v => FromString(v))
    {
    }

    private static UserAuditFailure FromString(string value) => value switch
    {
        "incorrect_password" => UserAuditFailure.IncorrectPassword,
        "no_password_set" => UserAuditFailure.NoPasswordSet,
        "user_deactivated" => UserAuditFailure.UserDeactivated,
        _ => throw new ArgumentException($"Invalid user audit failure '{value}'", nameof(value))
    };

    private static string ToString(UserAuditFailure failure) => failure switch
    {
        UserAuditFailure.IncorrectPassword => "incorrect_password",
        UserAuditFailure.NoPasswordSet => "no_password_set",
        UserAuditFailure.UserDeactivated => "user_deactivated",
        _ => throw new ArgumentOutOfRangeException(nameof(failure), failure, null)
    };
}
