using Buttercup.EntityModel;
using Buttercup.TestUtils;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Buttercup.DataAccess;

[Collection(nameof(DatabaseCollection))]
public sealed class RecipeDataProviderTests
{
    private readonly DatabaseCollectionFixture databaseFixture;
    private readonly ModelFactory modelFactory = new();

    private readonly StoppedClock clock = new();
    private readonly RecipeDataProvider recipeDataProvider;

    public RecipeDataProviderTests(DatabaseCollectionFixture databaseFixture)
    {
        this.databaseFixture = databaseFixture;
        this.recipeDataProvider = new(this.clock);
        this.clock.UtcNow = this.modelFactory.NextDateTime();
    }

    #region AddRecipe

    [Fact]
    public async Task AddRecipeInsertsRecipeAndReturnsId()
    {
        using var dbContext = this.databaseFixture.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        var currentUser = this.modelFactory.BuildUser();
        dbContext.Users.Add(currentUser);
        await dbContext.SaveChangesAsync();

        var attributes = this.modelFactory.BuildRecipeAttributes(setOptionalAttributes: true);

        var id = await this.recipeDataProvider.AddRecipe(dbContext, attributes, currentUser.Id);

        dbContext.ChangeTracker.Clear();

        var expected = new Recipe
        {
            Id = id,
            Title = attributes.Title,
            PreparationMinutes = attributes.PreparationMinutes,
            CookingMinutes = attributes.CookingMinutes,
            Servings = attributes.Servings,
            Ingredients = attributes.Ingredients,
            Method = attributes.Method,
            Suggestions = attributes.Suggestions,
            Remarks = attributes.Remarks,
            Source = attributes.Source,
            Created = this.clock.UtcNow,
            CreatedByUserId = currentUser.Id,
            Modified = this.clock.UtcNow,
            ModifiedByUserId = currentUser.Id,
            Revision = 0,
        };

        var actual = await dbContext.Recipes.FindAsync(id);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task AddRecipeAcceptsNullForOptionalAttributes()
    {
        using var dbContext = this.databaseFixture.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        var currentUser = this.modelFactory.BuildUser();
        dbContext.Users.Add(currentUser);
        await dbContext.SaveChangesAsync();

        var attributes = this.modelFactory.BuildRecipeAttributes(setOptionalAttributes: false);

        var id = await this.recipeDataProvider.AddRecipe(dbContext, attributes, currentUser.Id);

        dbContext.ChangeTracker.Clear();

        var actual = await dbContext.Recipes.FindAsync(id);

        Assert.NotNull(actual);
        Assert.Null(actual.PreparationMinutes);
        Assert.Null(actual.CookingMinutes);
        Assert.Null(actual.Servings);
        Assert.Null(actual.Suggestions);
        Assert.Null(actual.Remarks);
        Assert.Null(actual.Source);
    }

    #endregion

    #region DeleteRecipe

    [Fact]
    public async Task DeleteRecipeReturnsRecipe()
    {
        using var dbContext = this.databaseFixture.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        var recipe = this.modelFactory.BuildRecipe();
        dbContext.Recipes.Add(recipe);
        await dbContext.SaveChangesAsync();

        await this.recipeDataProvider.DeleteRecipe(dbContext, recipe.Id);

        Assert.False(await dbContext.Recipes.AnyAsync());
    }

    [Fact]
    public async Task DeleteRecipeThrowsIfRecordNotFound()
    {
        using var dbContext = this.databaseFixture.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        dbContext.Recipes.Add(this.modelFactory.BuildRecipe());
        await dbContext.SaveChangesAsync();

        var id = this.modelFactory.NextInt();

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => this.recipeDataProvider.DeleteRecipe(dbContext, id));

        Assert.Equal($"Recipe {id} not found", exception.Message);
    }

    #endregion

    #region GetAllRecipes

    [Fact]
    public async Task GetAllRecipesReturnsAllRecipesInTitleOrder()
    {
        using var dbContext = this.databaseFixture.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        var recipeB = this.modelFactory.BuildRecipe() with { Title = "recipe-title-b" };
        var recipeC = this.modelFactory.BuildRecipe() with { Title = "recipe-title-c" };
        var recipeA = this.modelFactory.BuildRecipe() with { Title = "recipe-title-a" };

        dbContext.Recipes.AddRange(recipeB, recipeC, recipeA);
        await dbContext.SaveChangesAsync();

        var expected = new Recipe[] {
            recipeA,
            recipeB,
            recipeC,
        };

        var actual = await this.recipeDataProvider.GetAllRecipes(dbContext);

        Assert.Equal(expected, actual);
    }

    #endregion

    #region GetRecipe

    [Fact]
    public async Task GetRecipeReturnsRecipe()
    {
        using var dbContext = this.databaseFixture.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        var expected = this.modelFactory.BuildRecipe();
        dbContext.Recipes.Add(expected);
        await dbContext.SaveChangesAsync();

        dbContext.ChangeTracker.Clear();

        var actual = await this.recipeDataProvider.GetRecipe(dbContext, expected.Id);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task GetRecipeThrowsIfRecordNotFound()
    {
        using var dbContext = this.databaseFixture.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        dbContext.Recipes.Add(this.modelFactory.BuildRecipe());
        await dbContext.SaveChangesAsync();

        var id = this.modelFactory.NextInt();

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => this.recipeDataProvider.GetRecipe(dbContext, id));

        Assert.Equal($"Recipe {id} not found", exception.Message);
    }

    #endregion

    #region GetRecentlyAddedRecipes

    [Fact]
    public async Task GetRecentlyAddedRecipesReturnsRecipesInReverseChronologicalOrder()
    {
        using var dbContext = this.databaseFixture.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        var allRecipes = new List<Recipe>();

        for (var i = 0; i < 15; i++)
        {
            allRecipes.Add(this.modelFactory.BuildRecipe() with
            {
                Created = new DateTime(2010, 1, 2, 3, 4, 5).AddHours(36 * i),
            });
        }

        dbContext.Recipes.AddRange(allRecipes);
        await dbContext.SaveChangesAsync();

        var expected = allRecipes.AsEnumerable().Reverse().Take(10);

        var actual = await this.recipeDataProvider.GetRecentlyAddedRecipes(dbContext);

        Assert.Equal(expected, actual);
    }

    #endregion

    #region GetRecentlyUpdatedRecipes

    [Fact]
    public async Task GetRecentlyUpdatedRecipesReturnsRecipesInReverseChronologicalOrder()
    {
        using var dbContext = this.databaseFixture.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        var baseDateTime = this.modelFactory.NextDateTime();

        Recipe BuildRecipe(int createdDaysAgo, int modifiedDaysAgo) =>
            this.modelFactory.BuildRecipe() with
            {
                Created = baseDateTime.AddDays(-createdDaysAgo),
                Modified = baseDateTime.AddDays(-modifiedDaysAgo),
            };

        var allRecipes = new[]
        {
            BuildRecipe(0, 12),
            BuildRecipe(0, 11),
            BuildRecipe(0, 1),
            BuildRecipe(0, 3), // explicitly excluded
            BuildRecipe(1, 13),
            BuildRecipe(1, 2),
            BuildRecipe(7, 7), // never-updated
            BuildRecipe(1, 14),
            BuildRecipe(0, 5), // explicitly excluded
            BuildRecipe(0, 6),
            BuildRecipe(1, 16),
            BuildRecipe(4, 4), // never-updated
            BuildRecipe(1, 8),
            BuildRecipe(2, 10),
            BuildRecipe(2, 9),
            BuildRecipe(1, 15),
        };

        dbContext.Recipes.AddRange(allRecipes);
        await dbContext.SaveChangesAsync();

        var expected = new[]
        {
            allRecipes[2],
            allRecipes[5],
            allRecipes[9],
            allRecipes[12],
            allRecipes[14],
            allRecipes[13],
            allRecipes[1],
            allRecipes[0],
            allRecipes[4],
            allRecipes[7],
        };

        var actual = await this.recipeDataProvider.GetRecentlyUpdatedRecipes(
            dbContext, new[] { allRecipes[3].Id, allRecipes[8].Id });

        Assert.Equal(expected, actual);
    }

    #endregion

    #region UpdateRecipe

    [Fact]
    public async Task UpdateRecipeUpdatesAllUpdatableAttributes()
    {
        using var dbContext = this.databaseFixture.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        var original = this.modelFactory.BuildRecipe(setOptionalAttributes: true);
        var currentUser = this.modelFactory.BuildUser();

        dbContext.AddRange(original, currentUser);
        await dbContext.SaveChangesAsync();

        var newAttributes = this.modelFactory.BuildRecipeAttributes(setOptionalAttributes: true);

        await this.recipeDataProvider.UpdateRecipe(
            dbContext, original.Id, newAttributes, original.Revision, currentUser.Id);

        dbContext.ChangeTracker.Clear();

        var expected = new Recipe
        {
            Id = original.Id,
            Title = newAttributes.Title,
            PreparationMinutes = newAttributes.PreparationMinutes,
            CookingMinutes = newAttributes.CookingMinutes,
            Servings = newAttributes.Servings,
            Ingredients = newAttributes.Ingredients,
            Method = newAttributes.Method,
            Suggestions = newAttributes.Suggestions,
            Remarks = newAttributes.Remarks,
            Source = newAttributes.Source,
            Created = original.Created,
            CreatedByUserId = original.CreatedByUserId,
            Modified = this.clock.UtcNow,
            ModifiedByUserId = currentUser.Id,
            Revision = original.Revision + 1,
        };

        var actual = await dbContext.Recipes.FindAsync(original.Id);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task UpdateRecipeAcceptsNullForOptionalAttributes()
    {
        using var dbContext = this.databaseFixture.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        var original = this.modelFactory.BuildRecipe(setOptionalAttributes: true);
        var currentUser = this.modelFactory.BuildUser();

        dbContext.AddRange(original, currentUser);
        await dbContext.SaveChangesAsync();

        var newAttributes = this.modelFactory.BuildRecipeAttributes(setOptionalAttributes: false);

        await this.recipeDataProvider.UpdateRecipe(
            dbContext, original.Id, newAttributes, original.Revision, currentUser.Id);

        dbContext.ChangeTracker.Clear();

        var actual = await dbContext.Recipes.FindAsync(original.Id);

        Assert.NotNull(actual);
        Assert.Null(actual.PreparationMinutes);
        Assert.Null(actual.CookingMinutes);
        Assert.Null(actual.Servings);
        Assert.Null(actual.Suggestions);
        Assert.Null(actual.Remarks);
        Assert.Null(actual.Source);
    }

    [Fact]
    public async Task UpdateRecipeThrowsIfRecordNotFound()
    {
        using var dbContext = this.databaseFixture.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        var otherRecipe = this.modelFactory.BuildRecipe();
        var currentUser = this.modelFactory.BuildUser();

        dbContext.AddRange(otherRecipe, currentUser);

        var id = this.modelFactory.NextInt();

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => this.recipeDataProvider.UpdateRecipe(
                dbContext, id, this.modelFactory.BuildRecipeAttributes(), 0, currentUser.Id));

        Assert.Equal($"Recipe {id} not found", exception.Message);
    }

    [Fact]
    public async Task UpdateRecipeThrowsIfRevisionOutOfSync()
    {
        using var dbContext = this.databaseFixture.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        var recipe = this.modelFactory.BuildRecipe(setOptionalAttributes: true);
        var currentUser = this.modelFactory.BuildUser();

        dbContext.AddRange(recipe, currentUser);
        await dbContext.SaveChangesAsync();

        var staleRevision = recipe.Revision - 1;

        var exception = await Assert.ThrowsAsync<ConcurrencyException>(
            () => this.recipeDataProvider.UpdateRecipe(
                dbContext,
                recipe.Id,
                this.modelFactory.BuildRecipeAttributes(),
                staleRevision,
                currentUser.Id));

        Assert.Equal(
            $"Revision {staleRevision} does not match current revision {recipe.Revision}",
            exception.Message);
    }

    #endregion
}
