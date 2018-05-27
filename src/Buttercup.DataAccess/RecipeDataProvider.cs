using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Buttercup.Models;

namespace Buttercup.DataAccess
{
    /// <summary>
    /// The default implementation of <see cref="IRecipeDataProvider" />.
    /// </summary>
    internal sealed class RecipeDataProvider : IRecipeDataProvider
    {
        /// <inheritdoc />
        public async Task<Recipe> GetRecipe(DbConnection connection, long id)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM recipe WHERE id = @id";
                command.AddParameterWithValue("@id", id);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (!await reader.ReadAsync())
                    {
                        throw new NotFoundException($"Recipe {id} not found");
                    }

                    return ReadRecipe(reader);
                }
            }
        }

        /// <inheritdoc />
        public Task<IList<Recipe>> GetRecipes(DbConnection connection) =>
            GetRecipes(connection, "SELECT * FROM recipe ORDER BY title");

        /// <inheritdoc />
        public Task<IList<Recipe>> GetRecentlyAddedRecipes(DbConnection connection) =>
            GetRecipes(connection, "SELECT * FROM recipe ORDER BY created DESC LIMIT 10");

        private static async Task<IList<Recipe>> GetRecipes(DbConnection connection, string query)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = query;

                using (var reader = await command.ExecuteReaderAsync())
                {
                    var recipes = new List<Recipe>();

                    while (await reader.ReadAsync())
                    {
                        recipes.Add(ReadRecipe(reader));
                    }

                    return recipes;
                }
            }
        }

        private static Recipe ReadRecipe(DbDataReader reader) =>
            new Recipe
            {
                Id = reader.GetInt64("id"),
                Title = reader.GetString("title"),
                PreparationMinutes = reader.GetNullableInt32("preparation_minutes"),
                CookingMinutes = reader.GetNullableInt32("cooking_minutes"),
                Servings = reader.GetNullableInt32("servings"),
                Ingredients = reader.GetString("ingredients"),
                Method = reader.GetString("method"),
                Suggestions = reader.GetString("suggestions"),
                Remarks = reader.GetString("remarks"),
                Source = reader.GetString("source"),
                Created = reader.GetDateTime("created", DateTimeKind.Utc),
                Modified = reader.GetDateTime("modified", DateTimeKind.Utc),
                Revision = reader.GetInt32("revision"),
            };
    }
}
