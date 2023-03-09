using Buttercup.Models;

namespace Buttercup.TestUtils;

/// <summary>
/// Provides methods for instantiating dummy models in tests.
/// </summary>
public class ModelFactory
{
    private int counter;

    /// <summary>
    /// Instantiates a new <see cref="Recipe" /> object with unique property values.
    /// </summary>
    /// <param name="includeOptionalAttributes">
    /// <c>true</c> if optional properties should be populated; <c>false</c> if they should be left
    /// null.
    /// </param>
    /// <returns>The new <see cref="Recipe" /> object.</returns>
    public Recipe CreateRecipe(bool includeOptionalAttributes = false)
    {
        var i = ++counter;

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

    /// <summary>
    /// Instantiates a new <see cref="RecipeAttributes" /> object with unique property values.
    /// </summary>
    /// <param name="includeOptionalAttributes">
    /// <c>true</c> if optional properties should be populated; <c>false</c> if they should be left
    /// null.
    /// </param>
    /// <returns>The new <see cref="RecipeAttributes" /> object.</returns>
    public RecipeAttributes CreateRecipeAttributes(bool includeOptionalAttributes = false) =>
        new(CreateRecipe(includeOptionalAttributes));

    /// <summary>
    /// Instantiates a new <see cref="User" /> object with unique property values.
    /// </summary>
    /// <param name="includeOptionalAttributes">
    /// <c>true</c> if optional properties should be populated; <c>false</c> if they should be left
    /// null.
    /// </param>
    /// <returns>The new <see cref="User" /> object.</returns>
    public User CreateUser(bool includeOptionalAttributes = false)
    {
        var i = ++counter;

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
