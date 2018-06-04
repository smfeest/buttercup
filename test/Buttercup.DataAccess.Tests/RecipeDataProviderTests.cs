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

        #region AddRecipe

        [Fact]
        public Task AddRecipeInsertsRecipeAndReturnsId() =>
            this.databaseFixture.WithRollback(async connection =>
        {
            var expected = CreateSampleRecipe(includeOptionalAttributes: true);

            var recipeDataProvider = new RecipeDataProvider();
            var id = await recipeDataProvider.AddRecipe(connection, expected);
            var actual = await recipeDataProvider.GetRecipe(connection, id);

            Assert.Equal(expected.Title, actual.Title);
            Assert.Equal(expected.PreparationMinutes, actual.PreparationMinutes);
            Assert.Equal(expected.CookingMinutes, actual.CookingMinutes);
            Assert.Equal(expected.Servings, actual.Servings);
            Assert.Equal(expected.Ingredients, actual.Ingredients);
            Assert.Equal(expected.Method, actual.Method);
            Assert.Equal(expected.Suggestions, actual.Suggestions);
            Assert.Equal(expected.Remarks, actual.Remarks);
            Assert.Equal(expected.Source, actual.Source);
        });

        [Fact]
        public Task AddRecipeAcceptsNullForOptionalAttributes() =>
            this.databaseFixture.WithRollback(async connection =>
        {
            var expected = CreateSampleRecipe(includeOptionalAttributes: false);

            var recipeDataProvider = new RecipeDataProvider();
            var id = await recipeDataProvider.AddRecipe(connection, expected);
            var actual = await recipeDataProvider.GetRecipe(connection, id);

            Assert.Null(actual.PreparationMinutes);
            Assert.Null(actual.CookingMinutes);
            Assert.Null(actual.Servings);
            Assert.Null(actual.Suggestions);
            Assert.Null(actual.Remarks);
            Assert.Null(actual.Source);
        });

        [Fact]
        public Task AddRecipeTrimsStringValues() =>
            this.databaseFixture.WithRollback(async connection =>
        {
            var expected = CreateSampleRecipe();

            expected.Title = " new-recipe-title ";
            expected.Ingredients = " new-recipe-ingredients ";
            expected.Method = " new-recipe-method ";
            expected.Suggestions = string.Empty;
            expected.Remarks = " ";
            expected.Source = string.Empty;

            var recipeDataProvider = new RecipeDataProvider();
            var id = await recipeDataProvider.AddRecipe(connection, expected);
            var actual = await recipeDataProvider.GetRecipe(connection, id);

            Assert.Equal("new-recipe-title", actual.Title);
            Assert.Equal("new-recipe-ingredients", actual.Ingredients);
            Assert.Equal("new-recipe-method", actual.Method);
            Assert.Null(actual.Suggestions);
            Assert.Null(actual.Remarks);
            Assert.Null(actual.Source);
        });

        [Fact]
        public Task AddRecipeSetsCreatedAndModifiedTimes() =>
            this.databaseFixture.WithRollback(async connection =>
        {
            var startTime = DateTime.UtcNow;

            var recipeDataProvider = new RecipeDataProvider();
            var id = await recipeDataProvider.AddRecipe(connection, CreateSampleRecipe());
            var recipe = await recipeDataProvider.GetRecipe(connection, id);

            AssertInRangeWithRounding(recipe.Created, startTime, DateTime.UtcNow);
            Assert.Equal(recipe.Created, recipe.Modified);
        });

        #endregion

        #region GetRecipe

        [Fact]
        public async Task GetRecipeReturnsRecipe() =>
            await this.databaseFixture.WithRollback(async connection =>
        {
            var expected = CreateSampleRecipe(id: 5);

            await InsertSampleRecipe(connection, expected);

            var actual = await new RecipeDataProvider().GetRecipe(connection, 5);

            Assert.Equal(5, actual.Id);
            Assert.Equal(expected.Title, actual.Title);
        });

        [Fact]
        public async Task GetRecipeThrowsIfRecordNotFound() =>
            await this.databaseFixture.WithRollback(async connection =>
        {
            await InsertSampleRecipe(connection, CreateSampleRecipe(id: 5));

            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => new RecipeDataProvider().GetRecipe(connection, 3));

            Assert.Equal("Recipe 3 not found", exception.Message);
        });

        #endregion

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

        #region GetRecentlyAddedRecipes

        [Fact]
        public Task GetRecentlyAddedRecipesReturnsRecipesInReverseChronologicalOrder() =>
            this.databaseFixture.WithRollback(async connection =>
        {
            for (var i = 1; i <= 15; i++)
            {
                var recipe = CreateSampleRecipe(title: $"recipe-{i}-title");
                recipe.Created = new DateTime(2010, 1, 2, 3, 4, 5).AddHours(36 * i);
                await InsertSampleRecipe(connection, recipe);
            }

            var recipes = await new RecipeDataProvider().GetRecentlyAddedRecipes(connection);

            Assert.Equal(10, recipes.Count);

            for (var i = 0; i < 10; i++)
            {
                Assert.Equal($"recipe-{15 - i}-title", recipes[i].Title);
            }
        });

        #endregion

        #region GetRecentlyUpdatedRecipes

        [Fact]
        public Task GetRecentlyUpdatedRecipesReturnsRecipesInReverseChronologicalOrder() =>
            this.databaseFixture.WithRollback(async connection =>
        {
            for (var i = 1; i <= 10; i++)
            {
                var recipe = CreateSampleRecipe(title: $"recently-updated-{i}");
                recipe.Created = new DateTime(2010, 1, 2, 3, 4, 5);
                recipe.Modified = new DateTime(2016, 7, i, 9, 10, 11);
                await InsertSampleRecipe(connection, recipe);
            }

            for (var i = 1; i <= 5; i++)
            {
                var recipe = CreateSampleRecipe(title: $"recently-created-never-updated-{i}");
                recipe.Created = recipe.Modified = new DateTime(2016, 8, i, 9, 10, 11);
                await InsertSampleRecipe(connection, recipe);
            }

            for (var i = 1; i <= 15; i++)
            {
                var recipe = CreateSampleRecipe(title: $"recently-created-and-updated-{i}");
                recipe.Created = new DateTime(2016, 9, i, 9, 10, 11);
                recipe.Modified = new DateTime(2016, 10, i, 9, 10, 11);
                await InsertSampleRecipe(connection, recipe);
            }

            var recipes = await new RecipeDataProvider().GetRecentlyUpdatedRecipes(connection);

            Assert.Equal(10, recipes.Count);

            for (var i = 0; i < 5; i++)
            {
                Assert.Equal($"recently-created-and-updated-{5 - i}", recipes[i].Title);
            }

            for (var i = 0; i < 5; i++)
            {
                Assert.Equal($"recently-updated-{10 - i}", recipes[5 + i].Title);
            }
        });

        #endregion

        #region UpdateRecipe

        [Fact]
        public Task UpdateRecipeUpdatesAllUpdatableAttributes() =>
            this.databaseFixture.WithRollback(async connection =>
        {
            await InsertSampleRecipe(connection, CreateSampleRecipe(id: 3, revision: 0));

            var expected = CreateSampleRecipe(includeOptionalAttributes: true, id: 3, revision: 0);

            var recipeDataProvider = new RecipeDataProvider();
            await recipeDataProvider.UpdateRecipe(connection, expected);
            var actual = await recipeDataProvider.GetRecipe(connection, 3);

            Assert.Equal(expected.Title, actual.Title);
            Assert.Equal(expected.PreparationMinutes, actual.PreparationMinutes);
            Assert.Equal(expected.CookingMinutes, actual.CookingMinutes);
            Assert.Equal(expected.Servings, actual.Servings);
            Assert.Equal(expected.Ingredients, actual.Ingredients);
            Assert.Equal(expected.Method, actual.Method);
            Assert.Equal(expected.Suggestions, actual.Suggestions);
            Assert.Equal(expected.Remarks, actual.Remarks);
            Assert.Equal(expected.Source, actual.Source);
        });

        [Fact]
        public Task UpdateRecipeAcceptsNullForOptionalAttributes() =>
            this.databaseFixture.WithRollback(async connection =>
        {
            await InsertSampleRecipe(
                connection,
                CreateSampleRecipe(includeOptionalAttributes: true, id: 7, revision: 3));

            var recipeDataProvider = new RecipeDataProvider();
            await recipeDataProvider.UpdateRecipe(
                connection,
                CreateSampleRecipe(includeOptionalAttributes: false, id: 7, revision: 3));

            var actual = await recipeDataProvider.GetRecipe(connection, 7);

            Assert.Null(actual.PreparationMinutes);
            Assert.Null(actual.CookingMinutes);
            Assert.Null(actual.Servings);
            Assert.Null(actual.Suggestions);
            Assert.Null(actual.Remarks);
            Assert.Null(actual.Source);
        });

        [Fact]
        public Task UpdateRecipeTrimsStringValues() =>
            this.databaseFixture.WithRollback(async connection =>
        {
            await InsertSampleRecipe(connection, CreateSampleRecipe(id: 13, revision: 0));

            var expected = CreateSampleRecipe(id: 13, revision: 0);
            expected.Title = " new-recipe-title ";
            expected.Ingredients = " new-recipe-ingredients ";
            expected.Method = " new-recipe-method ";
            expected.Suggestions = string.Empty;
            expected.Remarks = " ";
            expected.Source = string.Empty;

            var recipeDataProvider = new RecipeDataProvider();
            await recipeDataProvider.UpdateRecipe(connection, expected);
            var actual = await recipeDataProvider.GetRecipe(connection, 13);

            Assert.Equal("new-recipe-title", actual.Title);
            Assert.Equal("new-recipe-ingredients", actual.Ingredients);
            Assert.Equal("new-recipe-method", actual.Method);
            Assert.Null(actual.Suggestions);
            Assert.Null(actual.Remarks);
            Assert.Null(actual.Source);
        });

        [Fact]
        public Task UpdateRecipeSetsModifiedTimeOnly() =>
            this.databaseFixture.WithRollback(async connection =>
        {
            var original = CreateSampleRecipe(id: 2, revision: 3);
            await InsertSampleRecipe(connection, original);

            var startTime = DateTime.UtcNow;

            var recipeDataProvider = new RecipeDataProvider();
            await recipeDataProvider.UpdateRecipe(
                connection, CreateSampleRecipe(id: 2, revision: 3));
            var updated = await recipeDataProvider.GetRecipe(connection, original.Id);

            AssertInRangeWithRounding(updated.Modified, startTime, DateTime.UtcNow);
            Assert.Equal(original.Created, updated.Created);
        });

        [Fact]
        public async Task UpdateRecipeThrowsIfRecordNotFound() =>
            await this.databaseFixture.WithRollback(async connection =>
        {
            await InsertSampleRecipe(connection, CreateSampleRecipe(id: 5));

            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => new RecipeDataProvider().UpdateRecipe(connection, CreateSampleRecipe(id: 2)));

            Assert.Equal("Recipe 2 not found", exception.Message);
        });

        [Fact]
        public async Task UpdateRecipeThrowsIfRevisionOutOfSync() =>
            await this.databaseFixture.WithRollback(async connection =>
        {
            await InsertSampleRecipe(connection, CreateSampleRecipe(id: 6, revision: 4));

            var exception = await Assert.ThrowsAsync<ConcurrencyException>(
                () => new RecipeDataProvider().UpdateRecipe(
                    connection, CreateSampleRecipe(id: 6, revision: 3)));

            Assert.Equal("Revision 3 does not match current revision 4", exception.Message);
        });

        #endregion

        #region ReadRecipe

        [Fact]
        public Task ReadRecipeReadsAllAttributes() =>
            this.databaseFixture.WithRollback(async connection =>
        {
            var expected = CreateSampleRecipe(includeOptionalAttributes: true);

            await InsertSampleRecipe(connection, expected);

            var actual = await new RecipeDataProvider().GetRecipe(connection, expected.Id);

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

            var actual = await new RecipeDataProvider().GetRecipe(connection, expected.Id);

            Assert.Null(actual.PreparationMinutes);
            Assert.Null(actual.CookingMinutes);
            Assert.Null(actual.Servings);
            Assert.Null(actual.Suggestions);
            Assert.Null(actual.Remarks);
            Assert.Null(actual.Source);
        });

        #endregion

        private static Recipe CreateSampleRecipe(
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
                Created = new DateTime(2001, 2, 3, 4, 5, 6),
                Modified = new DateTime(2002, 3, 4, 5, 6, 7),
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

        private static void AssertInRangeWithRounding(DateTime actual, DateTime start, DateTime end)
        {
            Assert.InRange(actual, start.AddSeconds(-1), end.AddSeconds(1));
        }
    }
}
