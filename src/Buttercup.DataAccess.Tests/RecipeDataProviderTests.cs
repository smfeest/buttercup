using Buttercup.Models;
using Buttercup.TestUtils;
using Moq;
using Xunit;

namespace Buttercup.DataAccess;

[Collection("Database collection")]
public class RecipeDataProviderTests
{
    private readonly DateTime fakeTime = new(2020, 1, 2, 3, 4, 5);
    private readonly RecipeDataProvider recipeDataProvider;

    public RecipeDataProviderTests()
    {
        var clock = Mock.Of<IClock>(x => x.UtcNow == this.fakeTime);

        this.recipeDataProvider = new(clock);
    }

    #region AddRecipe

    [Fact]
    public async Task AddRecipeInsertsRecipeAndReturnsId()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        var currentUser = await new SampleDataHelper(connection).InsertUser();

        var recipeToAdd = ModelFactory.CreateRecipe(includeOptionalAttributes: true) with
        {
            CreatedByUserId = currentUser.Id
        };

        var id = await this.recipeDataProvider.AddRecipe(connection, recipeToAdd);

        var expected = recipeToAdd with
        {
            Id = id,
            Created = this.fakeTime,
            CreatedByUserId = currentUser.Id,
            Modified = this.fakeTime,
            ModifiedByUserId = currentUser.Id,
            Revision = 0,
        };

        var actual = await this.recipeDataProvider.GetRecipe(connection, id);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task AddRecipeAcceptsNullForOptionalAttributes()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        var recipeToAdd = ModelFactory.CreateRecipe(includeOptionalAttributes: false);

        var id = await this.recipeDataProvider.AddRecipe(connection, recipeToAdd);
        var actual = await this.recipeDataProvider.GetRecipe(connection, id);

        Assert.Null(actual.PreparationMinutes);
        Assert.Null(actual.CookingMinutes);
        Assert.Null(actual.Servings);
        Assert.Null(actual.Suggestions);
        Assert.Null(actual.Remarks);
        Assert.Null(actual.Source);
        Assert.Null(actual.CreatedByUserId);
        Assert.Null(actual.ModifiedByUserId);
    }

    [Fact]
    public async Task AddRecipeTrimsStringValues()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        var recipeToAdd = ModelFactory.CreateRecipe() with
        {
            Title = " new-recipe-title ",
            Ingredients = " new-recipe-ingredients ",
            Method = " new-recipe-method ",
            Suggestions = string.Empty,
            Remarks = " ",
            Source = string.Empty,
        };

        var id = await this.recipeDataProvider.AddRecipe(connection, recipeToAdd);
        var actual = await this.recipeDataProvider.GetRecipe(connection, id);

        Assert.Equal("new-recipe-title", actual.Title);
        Assert.Equal("new-recipe-ingredients", actual.Ingredients);
        Assert.Equal("new-recipe-method", actual.Method);
        Assert.Null(actual.Suggestions);
        Assert.Null(actual.Remarks);
        Assert.Null(actual.Source);
    }

    #endregion

    #region DeleteRecipe

    [Fact]
    public async Task DeleteRecipeReturnsRecipe()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        var recipe = await new SampleDataHelper(connection).InsertRecipe();

        await this.recipeDataProvider.DeleteRecipe(
            connection, recipe.Id, recipe.Revision);

        Assert.Empty(await this.recipeDataProvider.GetRecipes(connection));
    }

    [Fact]
    public async Task DeleteRecipeThrowsIfRecordNotFound()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        var otherRecipe = await new SampleDataHelper(connection).InsertRecipe();

        var id = otherRecipe.Id + 1;

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => this.recipeDataProvider.DeleteRecipe(connection, id, 0));

        Assert.Equal($"Recipe {id} not found", exception.Message);
    }

    [Fact]
    public async Task DeleteRecipeThrowsIfRevisionOutOfSync()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        var recipe = await new SampleDataHelper(connection).InsertRecipe();

        var staleRevision = recipe.Revision - 1;

        var exception = await Assert.ThrowsAsync<ConcurrencyException>(
            () => this.recipeDataProvider.DeleteRecipe(connection, recipe.Id, staleRevision));

        Assert.Equal(
            $"Revision {staleRevision} does not match current revision {recipe.Revision}",
            exception.Message);
    }

    #endregion

    #region GetRecipe

    [Fact]
    public async Task GetRecipeReturnsRecipe()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        var expected = await new SampleDataHelper(connection).InsertRecipe();

        var actual = await this.recipeDataProvider.GetRecipe(connection, expected.Id);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task GetRecipeThrowsIfRecordNotFound()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        var otherRecipe = await new SampleDataHelper(connection).InsertRecipe();

        var id = otherRecipe.Id + 1;

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => this.recipeDataProvider.GetRecipe(connection, id));

        Assert.Equal($"Recipe {id} not found", exception.Message);
    }

    #endregion

    #region GetRecipes

    [Fact]
    public async Task GetRecipesReturnsAllRecipesInTitleOrder()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        var sampleDataHelper = new SampleDataHelper(connection);

        var recipeB = await sampleDataHelper.InsertRecipe(
            ModelFactory.CreateRecipe() with { Title = "recipe-title-b" });
        var recipeC = await sampleDataHelper.InsertRecipe(
            ModelFactory.CreateRecipe() with { Title = "recipe-title-c" });
        var recipeA = await sampleDataHelper.InsertRecipe(
            ModelFactory.CreateRecipe() with { Title = "recipe-title-a" });

        var expected = new Recipe[] {
            recipeA,
            recipeB,
            recipeC,
        };

        var actual = await this.recipeDataProvider.GetRecipes(connection);

        Assert.Equal(expected, actual);
    }

    #endregion

    #region GetRecentlyAddedRecipes

    [Fact]
    public async Task GetRecentlyAddedRecipesReturnsRecipesInReverseChronologicalOrder()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        var sampleDataHelper = new SampleDataHelper(connection);

        for (var i = 1; i <= 15; i++)
        {
            await sampleDataHelper.InsertRecipe(ModelFactory.CreateRecipe() with
            {
                Title = $"recipe-{i}-title",
                Created = new DateTime(2010, 1, 2, 3, 4, 5).AddHours(36 * i),
            });
        }

        var recipes = await this.recipeDataProvider.GetRecentlyAddedRecipes(
            connection);

        Assert.Equal(10, recipes.Count);

        for (var i = 0; i < 10; i++)
        {
            Assert.Equal($"recipe-{15 - i}-title", recipes[i].Title);
        }
    }

    #endregion

    #region GetRecentlyUpdatedRecipes

    [Fact]
    public async Task GetRecentlyUpdatedRecipesReturnsRecipesInReverseChronologicalOrder()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        var sampleDataHelper = new SampleDataHelper(connection);

        for (var i = 1; i <= 10; i++)
        {
            await sampleDataHelper.InsertRecipe(ModelFactory.CreateRecipe() with
            {
                Title = $"recently-updated-{i}",
                Created = new(2010, 1, 2, 3, 4, 5),
                Modified = new(2016, 7, i, 9, 10, 11),
            });
        }

        for (var i = 1; i <= 5; i++)
        {
            var timestamp = new DateTime(2016, 8, i, 9, 10, 11);

            await sampleDataHelper.InsertRecipe(ModelFactory.CreateRecipe() with
            {
                Title = $"recently-created-never-updated-{i}",
                Created = timestamp,
                Modified = timestamp,
            });
        }

        for (var i = 1; i <= 15; i++)
        {
            await sampleDataHelper.InsertRecipe(ModelFactory.CreateRecipe() with
            {
                Title = $"recently-created-and-updated-{i}",
                Created = new(2016, 9, i, 9, 10, 11),
                Modified = new(2016, 10, i, 9, 10, 11),
            });
        }

        var recipes = await this.recipeDataProvider.GetRecentlyUpdatedRecipes(
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
    }

    #endregion

    #region UpdateRecipe

    [Fact]
    public async Task UpdateRecipeUpdatesAllUpdatableAttributes()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        var sampleDataHelper = new SampleDataHelper(connection);

        var original = await sampleDataHelper.InsertRecipe(includeOptionalAttributes: true);
        var currentUser = await sampleDataHelper.InsertUser();

        var recipeForUpdate = ModelFactory.CreateRecipe(includeOptionalAttributes: true) with
        {
            Id = original.Id,
            ModifiedByUserId = currentUser.Id,
            Revision = original.Revision,
        };

        await this.recipeDataProvider.UpdateRecipe(connection, recipeForUpdate);

        var expected = recipeForUpdate with
        {
            Created = original.Created,
            CreatedByUserId = original.CreatedByUserId,
            Modified = this.fakeTime,
            Revision = original.Revision + 1,
        };

        var actual = await this.recipeDataProvider.GetRecipe(connection, original.Id);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task UpdateRecipeAcceptsNullForOptionalAttributes()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        var sampleDataHelper = new SampleDataHelper(connection);

        var original = await sampleDataHelper.InsertRecipe(includeOptionalAttributes: true);

        var recipeForUpdate = ModelFactory.CreateRecipe(includeOptionalAttributes: false) with
        {
            Id = original.Id,
            Revision = original.Revision,
        };

        await this.recipeDataProvider.UpdateRecipe(connection, recipeForUpdate);

        var actual = await this.recipeDataProvider.GetRecipe(connection, original.Id);

        Assert.Null(actual.PreparationMinutes);
        Assert.Null(actual.CookingMinutes);
        Assert.Null(actual.Servings);
        Assert.Null(actual.Suggestions);
        Assert.Null(actual.Remarks);
        Assert.Null(actual.Source);
        Assert.Null(actual.ModifiedByUserId);
    }

    [Fact]
    public async Task UpdateRecipeTrimsStringValues()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        var original = await new SampleDataHelper(connection).InsertRecipe();

        var recipeForUpdate = original with
        {
            Title = " new-recipe-title ",
            Ingredients = " new-recipe-ingredients ",
            Method = " new-recipe-method ",
            Suggestions = string.Empty,
            Remarks = " ",
            Source = string.Empty,
        };

        await this.recipeDataProvider.UpdateRecipe(connection, recipeForUpdate);

        var actual = await this.recipeDataProvider.GetRecipe(connection, original.Id);

        Assert.Equal("new-recipe-title", actual.Title);
        Assert.Equal("new-recipe-ingredients", actual.Ingredients);
        Assert.Equal("new-recipe-method", actual.Method);
        Assert.Null(actual.Suggestions);
        Assert.Null(actual.Remarks);
        Assert.Null(actual.Source);
    }

    [Fact]
    public async Task UpdateRecipeThrowsIfRecordNotFound()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        var otherRecipe = await new SampleDataHelper(connection).InsertRecipe();

        var id = otherRecipe.Id + 1;

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => this.recipeDataProvider.UpdateRecipe(
                connection, ModelFactory.CreateRecipe() with { Id = id }));

        Assert.Equal($"Recipe {id} not found", exception.Message);
    }

    [Fact]
    public async Task UpdateRecipeThrowsIfRevisionOutOfSync()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        var recipe = await new SampleDataHelper(connection).InsertRecipe();

        var staleRevision = recipe.Revision - 1;

        var exception = await Assert.ThrowsAsync<ConcurrencyException>(
            () => this.recipeDataProvider.UpdateRecipe(
                connection, recipe with { Revision = staleRevision }));

        Assert.Equal(
            $"Revision {staleRevision} does not match current revision {recipe.Revision}",
            exception.Message);
    }

    #endregion

    #region ReadRecipe

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ReadRecipeReadsAllAttributes(bool includeOptionalAttributes)
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        var expected = await new SampleDataHelper(connection).InsertRecipe(
            includeOptionalAttributes);

        var actual = await this.recipeDataProvider.GetRecipe(connection, expected.Id);

        Assert.Equal(expected, actual);
    }

    #endregion
}
