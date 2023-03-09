using Buttercup.Models;
using Buttercup.TestUtils;
using Moq;
using Xunit;

namespace Buttercup.DataAccess;

[Collection("Database collection")]
public class RecipeDataProviderTests
{
    private readonly DateTime fakeTime = new(2020, 1, 2, 3, 4, 5);
    private readonly ModelFactory modelFactory = new();
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

        var attributes = this.modelFactory.BuildRecipeAttributes(setOptionalAttributes: true);

        var id = await this.recipeDataProvider.AddRecipe(connection, attributes, currentUser.Id);

        var expected = new Recipe(
            id,
            attributes.Title,
            attributes.PreparationMinutes,
            attributes.CookingMinutes,
            attributes.Servings,
            attributes.Ingredients,
            attributes.Method,
            attributes.Suggestions,
            attributes.Remarks,
            attributes.Source,
            this.fakeTime,
            currentUser.Id,
            this.fakeTime,
            currentUser.Id,
            0);

        var actual = await this.recipeDataProvider.GetRecipe(connection, id);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task AddRecipeAcceptsNullForOptionalAttributes()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        var currentUser = await new SampleDataHelper(connection).InsertUser();

        var attributes = this.modelFactory.BuildRecipeAttributes(setOptionalAttributes: false);

        var id = await this.recipeDataProvider.AddRecipe(connection, attributes, currentUser.Id);

        var actual = await this.recipeDataProvider.GetRecipe(connection, id);

        Assert.Null(actual.PreparationMinutes);
        Assert.Null(actual.CookingMinutes);
        Assert.Null(actual.Servings);
        Assert.Null(actual.Suggestions);
        Assert.Null(actual.Remarks);
        Assert.Null(actual.Source);
    }

    [Fact]
    public async Task AddRecipeTrimsStringValues()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        var currentUser = await new SampleDataHelper(connection).InsertUser();

        var attributes = this.modelFactory.BuildRecipeAttributes() with
        {
            Title = " new-recipe-title ",
            Ingredients = " new-recipe-ingredients ",
            Method = " new-recipe-method ",
            Suggestions = string.Empty,
            Remarks = " ",
            Source = string.Empty,
        };

        var id = await this.recipeDataProvider.AddRecipe(connection, attributes, currentUser.Id);
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

        await this.recipeDataProvider.DeleteRecipe(connection, recipe.Id);

        Assert.Empty(await this.recipeDataProvider.GetAllRecipes(connection));
    }

    [Fact]
    public async Task DeleteRecipeThrowsIfRecordNotFound()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        var otherRecipe = await new SampleDataHelper(connection).InsertRecipe();

        var id = otherRecipe.Id + 1;

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => this.recipeDataProvider.DeleteRecipe(connection, id));

        Assert.Equal($"Recipe {id} not found", exception.Message);
    }

    #endregion

    #region GetAllRecipes

    [Fact]
    public async Task GetAllRecipesReturnsAllRecipesInTitleOrder()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        var sampleDataHelper = new SampleDataHelper(connection);

        var recipeB = await sampleDataHelper.InsertRecipe(
            this.modelFactory.BuildRecipe() with { Title = "recipe-title-b" });
        var recipeC = await sampleDataHelper.InsertRecipe(
            this.modelFactory.BuildRecipe() with { Title = "recipe-title-c" });
        var recipeA = await sampleDataHelper.InsertRecipe(
            this.modelFactory.BuildRecipe() with { Title = "recipe-title-a" });

        var expected = new Recipe[] {
            recipeA,
            recipeB,
            recipeC,
        };

        var actual = await this.recipeDataProvider.GetAllRecipes(connection);

        Assert.Equal(expected, actual);
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

    #region GetRecentlyAddedRecipes

    [Fact]
    public async Task GetRecentlyAddedRecipesReturnsRecipesInReverseChronologicalOrder()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        var sampleDataHelper = new SampleDataHelper(connection);

        for (var i = 1; i <= 15; i++)
        {
            await sampleDataHelper.InsertRecipe(this.modelFactory.BuildRecipe() with
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
            await sampleDataHelper.InsertRecipe(this.modelFactory.BuildRecipe() with
            {
                Title = $"recently-updated-{i}",
                Created = new(2010, 1, 2, 3, 4, 5),
                Modified = new(2016, 7, i, 9, 10, 11),
            });
        }

        for (var i = 1; i <= 5; i++)
        {
            var timestamp = new DateTime(2016, 8, i, 9, 10, 11);

            await sampleDataHelper.InsertRecipe(this.modelFactory.BuildRecipe() with
            {
                Title = $"recently-created-never-updated-{i}",
                Created = timestamp,
                Modified = timestamp,
            });
        }

        for (var i = 1; i <= 15; i++)
        {
            await sampleDataHelper.InsertRecipe(this.modelFactory.BuildRecipe() with
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

    #region GetRecipes

    [Fact]
    public async Task GetRecipesReturnsRecipesWithMatchingIds()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        var sampleDataHelper = new SampleDataHelper(connection);

        var allRecipes = new[]
        {
            await sampleDataHelper.InsertRecipe(),
            await sampleDataHelper.InsertRecipe(),
            await sampleDataHelper.InsertRecipe(),
        };

        var expected = new[] { allRecipes[0], allRecipes[2] };

        var actual = await this.recipeDataProvider.GetRecipes(
            connection, new[] { allRecipes[0].Id, allRecipes[2].Id });

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task GetRecipesReturnsEmptyListWhenIdListIsEmpty()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        Assert.Empty(await this.recipeDataProvider.GetRecipes(connection, Array.Empty<long>()));
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

        var newAttributes = this.modelFactory.BuildRecipeAttributes(setOptionalAttributes: true);

        await this.recipeDataProvider.UpdateRecipe(
            connection, original.Id, newAttributes, original.Revision, currentUser.Id);

        var expected = new Recipe(
            original.Id,
            newAttributes.Title,
            newAttributes.PreparationMinutes,
            newAttributes.CookingMinutes,
            newAttributes.Servings,
            newAttributes.Ingredients,
            newAttributes.Method,
            newAttributes.Suggestions,
            newAttributes.Remarks,
            newAttributes.Source,
            original.Created,
            original.CreatedByUserId,
            this.fakeTime,
            currentUser.Id,
            original.Revision + 1);

        var actual = await this.recipeDataProvider.GetRecipe(connection, original.Id);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task UpdateRecipeAcceptsNullForOptionalAttributes()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        var sampleDataHelper = new SampleDataHelper(connection);

        var original = await sampleDataHelper.InsertRecipe(includeOptionalAttributes: true);
        var currentUser = await sampleDataHelper.InsertUser();

        var newAttributes = this.modelFactory.BuildRecipeAttributes(setOptionalAttributes: false);

        await this.recipeDataProvider.UpdateRecipe(
            connection, original.Id, newAttributes, original.Revision, currentUser.Id);

        var actual = await this.recipeDataProvider.GetRecipe(connection, original.Id);

        Assert.Null(actual.PreparationMinutes);
        Assert.Null(actual.CookingMinutes);
        Assert.Null(actual.Servings);
        Assert.Null(actual.Suggestions);
        Assert.Null(actual.Remarks);
        Assert.Null(actual.Source);
    }

    [Fact]
    public async Task UpdateRecipeTrimsStringValues()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        var sampleDataHelper = new SampleDataHelper(connection);

        var original = await sampleDataHelper.InsertRecipe();
        var currentUser = await sampleDataHelper.InsertUser();

        var newAttributes = this.modelFactory.BuildRecipeAttributes() with
        {
            Title = " new-recipe-title ",
            Ingredients = " new-recipe-ingredients ",
            Method = " new-recipe-method ",
            Suggestions = string.Empty,
            Remarks = " ",
            Source = string.Empty,
        };

        await this.recipeDataProvider.UpdateRecipe(
            connection, original.Id, newAttributes, original.Revision, currentUser.Id);

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

        var sampleDataHelper = new SampleDataHelper(connection);

        var otherRecipe = await sampleDataHelper.InsertRecipe();
        var currentUser = await sampleDataHelper.InsertUser();

        var id = otherRecipe.Id + 1;

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => this.recipeDataProvider.UpdateRecipe(
                connection, id, this.modelFactory.BuildRecipeAttributes(), 0, currentUser.Id));

        Assert.Equal($"Recipe {id} not found", exception.Message);
    }

    [Fact]
    public async Task UpdateRecipeThrowsIfRevisionOutOfSync()
    {
        using var connection = await TestDatabase.OpenConnectionWithRollback();

        var sampleDataHelper = new SampleDataHelper(connection);

        var recipe = await sampleDataHelper.InsertRecipe();
        var currentUser = await sampleDataHelper.InsertUser();

        var staleRevision = recipe.Revision - 1;

        var exception = await Assert.ThrowsAsync<ConcurrencyException>(
            () => this.recipeDataProvider.UpdateRecipe(
                connection,
                recipe.Id,
                this.modelFactory.BuildRecipeAttributes(),
                staleRevision,
                currentUser.Id));

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
