using Buttercup.Models;

namespace Buttercup.DataAccess;

public static class SampleRecipes
{
    private static int sampleRecipeCount;

    public static Recipe CreateSampleRecipe(
        bool includeOptionalAttributes = false,
        long? id = null,
        string? title = null,
        int? revision = null)
    {
        var i = ++sampleRecipeCount;

        var recipe = new Recipe
        {
            Id = id ?? i,
            Title = title ?? $"recipe-{i}-title",
            Ingredients = $"recipe-{i}-ingredients",
            Method = $"recipe-{i}-method",
            Created = new DateTime(2001, 2, 3, 4, 5, 6).AddSeconds(i),
            Modified = new DateTime(2002, 3, 4, 5, 6, 7).AddSeconds(i),
            Revision = revision ?? (i + 4),
        };

        if (includeOptionalAttributes)
        {
            recipe.PreparationMinutes = i + 1;
            recipe.CookingMinutes = i + 2;
            recipe.Servings = i + 3;
            recipe.Suggestions = $"recipe-{i}-suggestions";
            recipe.Remarks = $"recipe-{i}-remarks";
            recipe.Source = $"recipe-{i}-source";
            recipe.CreatedByUserId = i + 4;
            recipe.ModifiedByUserId = i + 5;
        }

        return recipe;
    }
}
