using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Buttercup.EntityModel;

internal sealed class RecipeActionToStringConverter : ValueConverter<RecipeAction, string>
{
    public RecipeActionToStringConverter() : base(v => ToString(v), v => FromString(v))
    {
    }

    private static RecipeAction FromString(string value) => value switch
    {
        "create" => RecipeAction.Create,
        "delete" => RecipeAction.Delete,
        "modify" => RecipeAction.Modify,
        _ => throw new ArgumentException($"Invalid action '{value}'", nameof(value))
    };

    private static string ToString(RecipeAction action) => action switch
    {
        RecipeAction.Create => "create",
        RecipeAction.Delete => "delete",
        RecipeAction.Modify => "modify",
        _ => throw new ArgumentOutOfRangeException(nameof(action), action, null)
    };
}
