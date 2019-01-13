using System;
using System.Data.Common;
using System.Threading.Tasks;
using Buttercup.Models;

namespace Buttercup.DataAccess
{
    public static class SampleRecipes
    {
        private static int sampleRecipeCount;

        public static Recipe CreateSampleRecipe(
            bool includeOptionalAttributes = false,
            long? id = null,
            string title = null,
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
            }

            return recipe;
        }

        public static async Task InsertSampleRecipe(DbConnection connection, Recipe recipe)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"INSERT recipe(id, title, preparation_minutes, cooking_minutes, servings, ingredients, method, suggestions, remarks, source, created, modified, revision)
                VALUES (@id, @title, @preparation_minutes, @cooking_minutes, @servings, @ingredients, @method, @suggestions, @remarks, @source, @created, @modified, @revision);";

                command.AddParameterWithValue("@id", recipe.Id);
                command.AddParameterWithValue("@title", recipe.Title);
                command.AddParameterWithValue("@preparation_minutes", recipe.PreparationMinutes);
                command.AddParameterWithValue("@cooking_minutes", recipe.CookingMinutes);
                command.AddParameterWithValue("@servings", recipe.Servings);
                command.AddParameterWithValue("@ingredients", recipe.Ingredients);
                command.AddParameterWithValue("@method", recipe.Method);
                command.AddParameterWithValue("@suggestions", recipe.Suggestions);
                command.AddParameterWithValue("@remarks", recipe.Remarks);
                command.AddParameterWithValue("@source", recipe.Source);
                command.AddParameterWithValue("@created", recipe.Created);
                command.AddParameterWithValue("@modified", recipe.Modified);
                command.AddParameterWithValue("@revision", recipe.Revision);

                await command.ExecuteNonQueryAsync();
            }
        }
    }
}
