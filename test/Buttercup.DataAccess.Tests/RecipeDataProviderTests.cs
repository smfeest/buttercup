using System;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace Buttercup.DataAccess
{
    [Collection("Database collection")]
    public class RecipeDataProviderTests
    {
        private readonly DatabaseFixture databaseFixture;

        public RecipeDataProviderTests(DatabaseFixture databaseFixture) =>
            this.databaseFixture = databaseFixture;

        #region AddRecipe

        [Fact]
        public Task AddRecipeInsertsRecipeAndReturnsId() =>
            this.databaseFixture.WithRollback(async connection =>
        {
            var recipeDataProvider = new RecipeDataProvider();

            var expected = SampleRecipes.CreateSampleRecipe(includeOptionalAttributes: true);

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

            var created = expected.Created;

            Assert.Equal(created, actual.Created);
            Assert.Equal(created, actual.Modified);
        });

        [Fact]
        public Task AddRecipeAcceptsNullForOptionalAttributes() =>
            this.databaseFixture.WithRollback(async connection =>
        {
            var recipeDataProvider = new RecipeDataProvider();

            var expected = SampleRecipes.CreateSampleRecipe(includeOptionalAttributes: false);

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
            var recipeDataProvider = new RecipeDataProvider();

            var expected = SampleRecipes.CreateSampleRecipe();

            expected.Title = " new-recipe-title ";
            expected.Ingredients = " new-recipe-ingredients ";
            expected.Method = " new-recipe-method ";
            expected.Suggestions = string.Empty;
            expected.Remarks = " ";
            expected.Source = string.Empty;

            var id = await recipeDataProvider.AddRecipe(connection, expected);
            var actual = await recipeDataProvider.GetRecipe(connection, id);

            Assert.Equal("new-recipe-title", actual.Title);
            Assert.Equal("new-recipe-ingredients", actual.Ingredients);
            Assert.Equal("new-recipe-method", actual.Method);
            Assert.Null(actual.Suggestions);
            Assert.Null(actual.Remarks);
            Assert.Null(actual.Source);
        });

        #endregion

        #region DeleteRecipe

        [Fact]
        public async Task DeleteRecipeReturnsRecipe() =>
            await this.databaseFixture.WithRollback(async connection =>
        {
            var recipeDataProvider = new RecipeDataProvider();

            await SampleRecipes.InsertSampleRecipe(
                connection, SampleRecipes.CreateSampleRecipe(id: 5, revision: 1));

            await recipeDataProvider.DeleteRecipe(connection, 5, 1);

            Assert.Empty(await recipeDataProvider.GetRecipes(connection));
        });

        [Fact]
        public async Task DeleteRecipeThrowsIfRecordNotFound() =>
            await this.databaseFixture.WithRollback(async connection =>
        {
            await SampleRecipes.InsertSampleRecipe(
                connection, SampleRecipes.CreateSampleRecipe(id: 1));

            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => new RecipeDataProvider().DeleteRecipe(connection, 2, 0));

            Assert.Equal("Recipe 2 not found", exception.Message);
        });

        [Fact]
        public async Task DeleteRecipeThrowsIfRevisionOutOfSync() =>
            await this.databaseFixture.WithRollback(async connection =>
        {
            await SampleRecipes.InsertSampleRecipe(
                connection, SampleRecipes.CreateSampleRecipe(id: 4, revision: 2));

            var exception = await Assert.ThrowsAsync<ConcurrencyException>(
                () => new RecipeDataProvider().DeleteRecipe(connection, 4, 1));

            Assert.Equal("Revision 1 does not match current revision 2", exception.Message);
        });

        #endregion

        #region GetRecipe

        [Fact]
        public async Task GetRecipeReturnsRecipe() =>
            await this.databaseFixture.WithRollback(async connection =>
        {
            var expected = SampleRecipes.CreateSampleRecipe(id: 5);

            await SampleRecipes.InsertSampleRecipe(connection, expected);

            var actual = await new RecipeDataProvider().GetRecipe(connection, 5);

            Assert.Equal(5, actual.Id);
            Assert.Equal(expected.Title, actual.Title);
        });

        [Fact]
        public async Task GetRecipeThrowsIfRecordNotFound() =>
            await this.databaseFixture.WithRollback(async connection =>
        {
            await SampleRecipes.InsertSampleRecipe(
                connection, SampleRecipes.CreateSampleRecipe(id: 5));

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
            await SampleRecipes.InsertSampleRecipe(
                connection, SampleRecipes.CreateSampleRecipe(title: "recipe-title-b"));
            await SampleRecipes.InsertSampleRecipe(
                connection, SampleRecipes.CreateSampleRecipe(title: "recipe-title-c"));
            await SampleRecipes.InsertSampleRecipe(
                connection, SampleRecipes.CreateSampleRecipe(title: "recipe-title-a"));

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
                var recipe = SampleRecipes.CreateSampleRecipe(title: $"recipe-{i}-title");
                recipe.Created = new DateTime(2010, 1, 2, 3, 4, 5).AddHours(36 * i);
                await SampleRecipes.InsertSampleRecipe(connection, recipe);
            }

            var recipes = await new RecipeDataProvider().GetRecentlyAddedRecipes(
                connection);

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
                var recipe = SampleRecipes.CreateSampleRecipe(title: $"recently-updated-{i}");
                recipe.Created = new DateTime(2010, 1, 2, 3, 4, 5);
                recipe.Modified = new DateTime(2016, 7, i, 9, 10, 11);
                await SampleRecipes.InsertSampleRecipe(connection, recipe);
            }

            for (var i = 1; i <= 5; i++)
            {
                var recipe = SampleRecipes.CreateSampleRecipe(
                    title: $"recently-created-never-updated-{i}");
                recipe.Created = recipe.Modified = new DateTime(2016, 8, i, 9, 10, 11);
                await SampleRecipes.InsertSampleRecipe(connection, recipe);
            }

            for (var i = 1; i <= 15; i++)
            {
                var recipe = SampleRecipes.CreateSampleRecipe(
                    title: $"recently-created-and-updated-{i}");
                recipe.Created = new DateTime(2016, 9, i, 9, 10, 11);
                recipe.Modified = new DateTime(2016, 10, i, 9, 10, 11);
                await SampleRecipes.InsertSampleRecipe(connection, recipe);
            }

            var recipes = await new RecipeDataProvider().GetRecentlyUpdatedRecipes(
                connection);

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
            var recipeDataProvider = new RecipeDataProvider();

            var original = SampleRecipes.CreateSampleRecipe(id: 3, revision: 0);

            await SampleRecipes.InsertSampleRecipe(connection, original);

            var expected = SampleRecipes.CreateSampleRecipe(
                includeOptionalAttributes: true, id: 3, revision: 0);

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
            Assert.Equal(expected.Modified, actual.Modified);

            Assert.Equal(original.Created, actual.Created);
        });

        [Fact]
        public Task UpdateRecipeAcceptsNullForOptionalAttributes() =>
            this.databaseFixture.WithRollback(async connection =>
        {
            var recipeDataProvider = new RecipeDataProvider();

            await SampleRecipes.InsertSampleRecipe(
                connection,
                SampleRecipes.CreateSampleRecipe(
                    includeOptionalAttributes: true, id: 7, revision: 3));

            await recipeDataProvider.UpdateRecipe(
                connection,
                SampleRecipes.CreateSampleRecipe(
                    includeOptionalAttributes: false, id: 7, revision: 3));

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
            var recipeDataProvider = new RecipeDataProvider();

            await SampleRecipes.InsertSampleRecipe(
                connection, SampleRecipes.CreateSampleRecipe(id: 13, revision: 0));

            var expected = SampleRecipes.CreateSampleRecipe(id: 13, revision: 0);
            expected.Title = " new-recipe-title ";
            expected.Ingredients = " new-recipe-ingredients ";
            expected.Method = " new-recipe-method ";
            expected.Suggestions = string.Empty;
            expected.Remarks = " ";
            expected.Source = string.Empty;

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
        public async Task UpdateRecipeThrowsIfRecordNotFound() =>
            await this.databaseFixture.WithRollback(async connection =>
        {
            await SampleRecipes.InsertSampleRecipe(
                connection, SampleRecipes.CreateSampleRecipe(id: 5));

            var exception = await Assert.ThrowsAsync<NotFoundException>(
                () => new RecipeDataProvider().UpdateRecipe(
                    connection, SampleRecipes.CreateSampleRecipe(id: 2)));

            Assert.Equal("Recipe 2 not found", exception.Message);
        });

        [Fact]
        public async Task UpdateRecipeThrowsIfRevisionOutOfSync() =>
            await this.databaseFixture.WithRollback(async connection =>
        {
            await SampleRecipes.InsertSampleRecipe(
                connection, SampleRecipes.CreateSampleRecipe(id: 6, revision: 4));

            var exception = await Assert.ThrowsAsync<ConcurrencyException>(
                () => new RecipeDataProvider().UpdateRecipe(
                    connection, SampleRecipes.CreateSampleRecipe(id: 6, revision: 3)));

            Assert.Equal("Revision 3 does not match current revision 4", exception.Message);
        });

        #endregion

        #region ReadRecipe

        [Fact]
        public Task ReadRecipeReadsAllAttributes() =>
            this.databaseFixture.WithRollback(async connection =>
        {
            var expected = SampleRecipes.CreateSampleRecipe(includeOptionalAttributes: true);

            await SampleRecipes.InsertSampleRecipe(connection, expected);

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
            var expected = SampleRecipes.CreateSampleRecipe(includeOptionalAttributes: false);

            await SampleRecipes.InsertSampleRecipe(connection, expected);

            var actual = await new RecipeDataProvider().GetRecipe(connection, expected.Id);

            Assert.Null(actual.PreparationMinutes);
            Assert.Null(actual.CookingMinutes);
            Assert.Null(actual.Servings);
            Assert.Null(actual.Suggestions);
            Assert.Null(actual.Remarks);
            Assert.Null(actual.Source);
        });

        #endregion
    }
}
