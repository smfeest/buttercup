using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Buttercup.Models;
using MySql.Data.MySqlClient;

namespace Buttercup.DataAccess
{
    /// <summary>
    /// The default implementation of <see cref="IRecipeDataProvider" />.
    /// </summary>
    internal sealed class RecipeDataProvider : IRecipeDataProvider
    {
        /// <inheritdoc />
        public async Task<long> AddRecipe(DbConnection connection, Recipe recipe)
        {
            using (var command = (MySqlCommand)connection.CreateCommand())
            {
                command.CommandText = @"INSERT recipe (title, preparation_minutes, cooking_minutes, servings, ingredients, method, suggestions, remarks, source, created, created_by_user_id, modified, modified_by_user_id)
VALUES (@title, @preparation_minutes, @cooking_minutes, @servings, @ingredients, @method, @suggestions, @remarks, @source, @created, @created_by_user_id, @created, @created_by_user_id)";

                AddInsertUpdateParameters(command, recipe);
                command.AddParameterWithValue("@created", recipe.Created);
                command.AddParameterWithValue("@created_by_user_id", recipe.CreatedByUserId);

                await command.ExecuteNonQueryAsync();

                return command.LastInsertedId;
            }
        }

        /// <inheritdoc />
        public async Task DeleteRecipe(DbConnection connection, long id, int revision)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText =
                    "DELETE FROM recipe WHERE id = @id AND revision = @revision";
                command.AddParameterWithValue("@id", id);
                command.AddParameterWithValue("@revision", revision);

                if (await command.ExecuteNonQueryAsync() == 0)
                {
                    throw await ConcurrencyOrNotFoundException(connection, id, revision);
                }
            }
        }

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

        /// <inheritdoc />
        public Task<IList<Recipe>> GetRecentlyUpdatedRecipes(DbConnection connection)
        {
            var query = @"SELECT *
                FROM recipe
                LEFT JOIN (SELECT id AS added_id FROM recipe ORDER BY created DESC LIMIT 10) AS added ON added_id = id
                WHERE created != modified AND added_id IS NULL
                ORDER BY modified DESC LIMIT 10";

            return GetRecipes(connection, query);
        }

        /// <inheritdoc />
        public async Task UpdateRecipe(DbConnection connection, Recipe recipe)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"UPDATE recipe
                    SET title = @title, preparation_minutes = @preparation_minutes,
                        cooking_minutes = @cooking_minutes, servings = @servings,
                        ingredients = @ingredients, method = @method, suggestions = @suggestions,
                        remarks = @remarks, source = @source, modified = @modified,
                        revision = revision + 1
                    WHERE id = @id AND revision = @revision";

                AddInsertUpdateParameters(command, recipe);
                command.AddParameterWithValue("@id", recipe.Id);
                command.AddParameterWithValue("@modified", recipe.Modified);
                command.AddParameterWithValue("@revision", recipe.Revision);

                if (await command.ExecuteNonQueryAsync() == 0)
                {
                    throw await ConcurrencyOrNotFoundException(
                        connection, recipe.Id, recipe.Revision);
                }
            }
        }

        private static void AddInsertUpdateParameters(DbCommand command, Recipe recipe)
        {
            command.AddParameterWithStringValue("@title", recipe.Title);
            command.AddParameterWithValue("@preparation_minutes", recipe.PreparationMinutes);
            command.AddParameterWithValue("@cooking_minutes", recipe.CookingMinutes);
            command.AddParameterWithValue("@servings", recipe.Servings);
            command.AddParameterWithStringValue("@ingredients", recipe.Ingredients);
            command.AddParameterWithStringValue("@method", recipe.Method);
            command.AddParameterWithStringValue("@suggestions", recipe.Suggestions);
            command.AddParameterWithStringValue("@remarks", recipe.Remarks);
            command.AddParameterWithStringValue("@source", recipe.Source);
        }

        private static async Task<Exception> ConcurrencyOrNotFoundException(
            DbConnection connection, long id, int revision)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT revision FROM recipe WHERE id = @id";
                command.AddParameterWithValue("@id", id);

                var currentRevision = await command.ExecuteScalarAsync();

                if (currentRevision == null)
                {
                    return new NotFoundException($"Recipe {id} not found");
                }
                else
                {
                    return new ConcurrencyException(
                        $"Revision {revision} does not match current revision {currentRevision}");
                }
            }
        }

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
                CreatedByUserId = reader.GetNullableInt64("created_by_user_id"),
                Modified = reader.GetDateTime("modified", DateTimeKind.Utc),
                ModifiedByUserId = reader.GetNullableInt64("modified_by_user_id"),
                Revision = reader.GetInt32("revision"),
            };
    }
}
