using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Buttercup.EntityModel;

internal sealed class CommentActionToStringConverter : ValueConverter<CommentAction, string>
{
    public CommentActionToStringConverter() : base(v => ToString(v), v => FromString(v))
    {
    }

    private static CommentAction FromString(string value) => value switch
    {
        "create" => CommentAction.Create,
        "delete" => CommentAction.Delete,
        _ => throw new ArgumentException($"Invalid action '{value}'", nameof(value))
    };

    private static string ToString(CommentAction action) => action switch
    {
        CommentAction.Create => "create",
        CommentAction.Delete => "delete",
        _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
    };
}
