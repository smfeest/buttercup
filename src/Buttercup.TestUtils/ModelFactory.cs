using Buttercup.Models;

namespace Buttercup.TestUtils;

public static class ModelFactory
{
    private static int counter;

    public static Recipe CreateRecipe(bool includeOptionalAttributes = false)
    {
        var i = Interlocked.Increment(ref counter);

        return new(
            i,
            $"recipe-{i}-title",
            includeOptionalAttributes ? i + 1 : null,
            includeOptionalAttributes ? i + 2 : null,
            includeOptionalAttributes ? i + 3 : null,
            $"recipe-{i}-ingredients",
            $"recipe-{i}-method",
            includeOptionalAttributes ? $"recipe-{i}-suggestions" : null,
            includeOptionalAttributes ? $"recipe-{i}-remarks" : null,
            includeOptionalAttributes ? $"recipe-{i}-source" : null,
            new DateTime(2001, 2, 3, 4, 5, 6).AddSeconds(i),
            includeOptionalAttributes ? i + 4 : null,
            new DateTime(2002, 3, 4, 5, 6, 7).AddSeconds(i),
            includeOptionalAttributes ? i + 5 : null,
            i + 4);
    }

    public static RecipeAttributes CreateRecipeAttributes(bool includeOptionalAttributes = false) =>
        new(CreateRecipe(includeOptionalAttributes));

    public static User CreateUser(bool includeOptionalAttributes = false)
    {
        var i = Interlocked.Increment(ref counter);

        return new(
            i,
            $"user-{i}-name",
             $"user-{i}@example.com",
            includeOptionalAttributes ? $"user-{i}-password" : null,
            includeOptionalAttributes ? new DateTime(2000, 1, 2, 3, 4, 5).AddSeconds(i) : null,
            "secstamp",
            $"user-{i}-time-zone",
            new DateTime(2001, 2, 3, 4, 5, 6).AddSeconds(i),
            new DateTime(2002, 3, 4, 5, 6, 7).AddSeconds(i),
            i + 1);
    }
}
