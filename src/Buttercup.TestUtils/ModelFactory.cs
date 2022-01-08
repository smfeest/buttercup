using Buttercup.Models;

namespace Buttercup.TestUtils;

public static class ModelFactory
{
    private static int counter;

    public static Recipe CreateRecipe(
        bool includeOptionalAttributes = false,
        long? id = null,
        string? title = null,
        int? revision = null)
    {
        var i = ++counter;

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

    public static User CreateUser(
        bool includeOptionalAttributes = false,
        long? id = null,
        string? email = null,
        int? revision = null)
    {
        var i = ++counter;

        var user = new User
        {
            Id = id ?? i,
            Name = $"user-{i}-name",
            Email = email ?? $"user-{i}@example.com",
            SecurityStamp = "secstamp",
            TimeZone = $"user-{i}-time-zone",
            Created = new DateTime(2001, 2, 3, 4, 5, 6).AddSeconds(i),
            Modified = new DateTime(2002, 3, 4, 5, 6, 7).AddSeconds(i),
            Revision = revision ?? (i + 1),
        };

        if (includeOptionalAttributes)
        {
            user.HashedPassword = $"user-{i}-password";
            user.PasswordCreated = new DateTime(2000, 1, 2, 3, 4, 5).AddSeconds(i);
        }

        return user;
    }
}
