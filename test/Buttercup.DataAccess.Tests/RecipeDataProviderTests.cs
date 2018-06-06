using System;
using System.Data.Common;
using System.Threading.Tasks;
using Buttercup.Models;
using Xunit;

namespace Buttercup.DataAccess
{
    [Collection("Database collection")]
    public class RecipeDataProviderTests
    {
        private static int sampleRecipeCount;

        private readonly DatabaseFixture databaseFixture;

        public RecipeDataProviderTests(DatabaseFixture databaseFixture) =>
            this.databaseFixture = databaseFixture;

        #region GetRecipes

        [Fact]
        public Task GetRecipesReturnsAllRecipesInTitleOrder() =>
            this.databaseFixture.WithRollback(async connection =>
        {
            await InsertSampleRecipe(connection, CreateSampleRecipe(title: "recipe-title-b"));
            await InsertSampleRecipe(connection, CreateSampleRecipe(title: "recipe-title-c"));
            await InsertSampleRecipe(connection, CreateSampleRecipe(title: "recipe-title-a"));

            var recipes = await new RecipeDataProvider().GetRecipes(connection);

            Assert.Equal("recipe-title-a", recipes[0].Title);
            Assert.Equal("recipe-title-b", recipes[1].Title);
            Assert.Equal("recipe-title-c", recipes[2].Title);
        });

        #endregion

        #region ReadRecipe

        [Fact]
        public Task ReadRecipeReadsAllAttributes() =>
            this.databaseFixture.WithRollback(async connection =>
        {
            var expected = CreateSampleRecipe(includeOptionalAttributes: true);

            await InsertSampleRecipe(connection, expected);

            var actual = await ReadRecipe(connection, expected.Id);

            Assert.Equal(expected.Id, actual.Id);
            Assert.Equal(expected.Title, actual.Title);
            Assert.Equal(expected.PreparationMinutes, actual.PreparationMinutes);
            Assert.Equal(expected.CookingMinutes, actual.CookingMinutes);
            Assert.Equal(expected.Servings, actual.Servings);
            Assert.Equal(expected.Ingredients, actual.Ingredients);
            Assert.Equal(expected.Method, actual.Method);
            Assert.Equal(expected.Suggestions, actual.Suggestions);
            Assert.Equal(expected.Remarks, actual.Remarks);
            Assert.Equal(expected.Source, actual.Source);
            Assert.Equal(expected.Created, actual.Created);
            Assert.Equal(DateTimeKind.Utc, actual.Created.Kind);
            Assert.Equal(expected.Modified, actual.Modified);
            Assert.Equal(DateTimeKind.Utc, actual.Modified.Kind);
            Assert.Equal(expected.Revision, actual.Revision);
        });

        [Fact]
        public Task ReadRecipeHandlesNullAttributes() =>
            this.databaseFixture.WithRollback(async connection =>
        {
            var expected = CreateSampleRecipe(includeOptionalAttributes: false);

            await InsertSampleRecipe(connection, expected);

            var actual = await ReadRecipe(connection, expected.Id);

            Assert.Null(actual.PreparationMinutes);
            Assert.Null(actual.CookingMinutes);
            Assert.Null(actual.Servings);
            Assert.Null(actual.Suggestions);
            Assert.Null(actual.Remarks);
            Assert.Null(actual.Source);
        });

        #endregion

        private static Recipe CreateSampleRecipe(
            bool includeOptionalAttributes = false, string title = null)
        {
            var i = ++sampleRecipeCount;

            var recipe = new Recipe
            {
                Id = i,
                Title = title ?? $"recipe-{i}-title",
                Ingredients = $"recipe-{i}-ingredients",
                Method = $"recipe-{i}-method",
                Created = new DateTime(2001, 2, 3, 4, 5, 6),
                Modified = new DateTime(2002, 3, 4, 5, 6, 7),
                Revision = i + 4,
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

        private static async Task InsertSampleRecipe(DbConnection connection, Recipe recipe)
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

        private static async Task<Recipe> ReadRecipe(DbConnection connection, long id)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT * FROM recipe WHERE id = @id";

                command.AddParameterWithValue("@id", id);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    await reader.ReadAsync();

                    return RecipeDataProvider.ReadRecipe(reader);
                }
            }
        }
    }
}
