using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Buttercup.EntityModel;

internal sealed class RoleToStringConverter : ValueConverter<Role, string>
{
    public RoleToStringConverter() : base(v => ToString(v), v => FromString(v))
    {
    }

    private static Role FromString(string value) => value switch
    {
        "admin" => Role.Admin,
        "contributor" => Role.Contributor,
        _ => throw new ArgumentException($"Invalid role '{value}'", nameof(value))
    };

    private static string ToString(Role role) => role switch
    {
        Role.Admin => "admin",
        Role.Contributor => "contributor",
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
    };
}
