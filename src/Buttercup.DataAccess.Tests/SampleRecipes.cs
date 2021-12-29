using Buttercup.Models;
using MySqlConnector;

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

    public static async Task InsertSampleRecipe(
        MySqlConnection connection, Recipe recipe, bool insertRelatedRecords = false)
    {
        if (insertRelatedRecords)
        {
            await InsertRelatedUser(connection, recipe.CreatedByUserId);
            await InsertRelatedUser(connection, recipe.ModifiedByUserId);
        }

        using var command = connection.CreateCommand();

        command.CommandText = @"INSERT recipe(id, title, preparation_minutes, cooking_minutes, servings, ingredients, method, suggestions, remarks, source, created, created_by_user_id, modified, modified_by_user_id, revision)
            VALUES (@id, @title, @preparation_minutes, @cooking_minutes, @servings, @ingredients, @method, @suggestions, @remarks, @source, @created, @created_by_user_id, @modified, @modified_by_user_id, @revision);";

        command.Parameters.AddWithValue("@id", recipe.Id);
        command.Parameters.AddWithValue("@title", recipe.Title);
        command.Parameters.AddWithValue("@preparation_minutes", recipe.PreparationMinutes);
        command.Parameters.AddWithValue("@cooking_minutes", recipe.CookingMinutes);
        command.Parameters.AddWithValue("@servings", recipe.Servings);
        command.Parameters.AddWithValue("@ingredients", recipe.Ingredients);
        command.Parameters.AddWithValue("@method", recipe.Method);
        command.Parameters.AddWithValue("@suggestions", recipe.Suggestions);
        command.Parameters.AddWithValue("@remarks", recipe.Remarks);
        command.Parameters.AddWithValue("@source", recipe.Source);
        command.Parameters.AddWithValue("@created", recipe.Created);
        command.Parameters.AddWithValue("@created_by_user_id", recipe.CreatedByUserId);
        command.Parameters.AddWithValue("@modified", recipe.Modified);
        command.Parameters.AddWithValue("@modified_by_user_id", recipe.ModifiedByUserId);
        command.Parameters.AddWithValue("@revision", recipe.Revision);

        await command.ExecuteNonQueryAsync();
    }

    private static async Task InsertRelatedUser(MySqlConnection connection, long? userId)
    {
        if (userId.HasValue)
        {
            await SampleUsers.InsertSampleUser(
                connection, SampleUsers.CreateSampleUser(id: userId));
        }
    }
}
