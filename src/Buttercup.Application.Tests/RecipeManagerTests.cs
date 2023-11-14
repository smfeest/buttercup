using Buttercup.EntityModel;
using Buttercup.TestUtils;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Buttercup.Application;

[Collection(nameof(DatabaseCollection))]
public sealed class RecipeManagerTests : IAsyncLifetime
{
    private readonly DatabaseCollectionFixture databaseFixture;
    private readonly ModelFactory modelFactory = new();

    private readonly StoppedClock clock = new();
    private readonly RecipeManager RecipeManager;

    public RecipeManagerTests(DatabaseCollectionFixture databaseFixture)
    {
        this.databaseFixture = databaseFixture;
        this.RecipeManager = new(this.clock, databaseFixture);
        this.clock.UtcNow = this.modelFactory.NextDateTime();
    }

    public Task InitializeAsync() => this.databaseFixture.ClearDatabase();

    public Task DisposeAsync() => Task.CompletedTask;

    #region AddRecipe

    [Fact]
    public async Task AddRecipe_InsertsRecipeAndReturnsId()
    {
        var currentUser = this.modelFactory.BuildUser();
        var attributes = new RecipeAttributes(
            this.modelFactory.BuildRecipe(setOptionalAttributes: true));

        using (var dbContext = this.databaseFixture.CreateDbContext())
        {
            dbContext.Users.Add(currentUser);
            await dbContext.SaveChangesAsync();
        }

        var id = await this.RecipeManager.AddRecipe(attributes, currentUser.Id);

        using (var dbContext = this.databaseFixture.CreateDbContext())
        {
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
    }

    [Fact]
    public async Task AddRecipe_AcceptsNullForOptionalAttributes()
    {
        var currentUser = this.modelFactory.BuildUser();
        var attributes = new RecipeAttributes(
            this.modelFactory.BuildRecipe(setOptionalAttributes: false));

        using (var dbContext = this.databaseFixture.CreateDbContext())
        {
            dbContext.Users.Add(currentUser);
            await dbContext.SaveChangesAsync();
        }

        var id = await this.RecipeManager.AddRecipe(attributes, currentUser.Id);

        using (var dbContext = this.databaseFixture.CreateDbContext())
        {
            var actual = await dbContext.Recipes.FindAsync(id);

            Assert.NotNull(actual);
            Assert.Null(actual.PreparationMinutes);
            Assert.Null(actual.CookingMinutes);
            Assert.Null(actual.Servings);
            Assert.Null(actual.Suggestions);
            Assert.Null(actual.Remarks);
            Assert.Null(actual.Source);
        }
    }

    #endregion

    #region DeleteRecipe

    [Fact]
    public async Task DeleteRecipe_ReturnsRecipe()
    {
        var recipe = this.modelFactory.BuildRecipe();

        using (var dbContext = this.databaseFixture.CreateDbContext())
        {
            dbContext.Recipes.Add(recipe);
            await dbContext.SaveChangesAsync();
        }

        await this.RecipeManager.DeleteRecipe(recipe.Id);

        using (var dbContext = this.databaseFixture.CreateDbContext())
        {
            Assert.False(await dbContext.Recipes.AnyAsync());
        }
    }

    [Fact]
    public async Task DeleteRecipe_ThrowsIfRecordNotFound()
    {
        using (var dbContext = this.databaseFixture.CreateDbContext())
        {
            dbContext.Recipes.Add(this.modelFactory.BuildRecipe());
            await dbContext.SaveChangesAsync();
        }

        var id = this.modelFactory.NextInt();

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => this.RecipeManager.DeleteRecipe(id));

        Assert.Equal($"Recipe {id} not found", exception.Message);
    }

    #endregion

    #region GetAllRecipes

    [Fact]
    public async Task GetAllRecipes_ReturnsAllRecipesInTitleOrder()
    {
        var recipeB = this.modelFactory.BuildRecipe() with { Title = "recipe-title-b" };
        var recipeC = this.modelFactory.BuildRecipe() with { Title = "recipe-title-c" };
        var recipeA = this.modelFactory.BuildRecipe() with { Title = "recipe-title-a" };

        using (var dbContext = this.databaseFixture.CreateDbContext())
        {
            dbContext.Recipes.AddRange(recipeB, recipeC, recipeA);
            await dbContext.SaveChangesAsync();
        }

        var expected = new Recipe[] {
            recipeA,
            recipeB,
            recipeC,
        };

        var actual = await this.RecipeManager.GetAllRecipes();

        Assert.Equal(expected, actual);
    }

    #endregion

    #region GetRecipe

    [Fact]
    public async Task GetRecipe_ReturnsRecipe()
    {
        var expected = this.modelFactory.BuildRecipe();

        using (var dbContext = this.databaseFixture.CreateDbContext())
        {
            dbContext.Recipes.Add(expected);
            await dbContext.SaveChangesAsync();
        }

        var actual = await this.RecipeManager.GetRecipe(expected.Id);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task GetRecipe_ThrowsIfRecordNotFound()
    {
        using (var dbContext = this.databaseFixture.CreateDbContext())
        {
            dbContext.Recipes.Add(this.modelFactory.BuildRecipe());
            await dbContext.SaveChangesAsync();
        }

        var id = this.modelFactory.NextInt();

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => this.RecipeManager.GetRecipe(id));

        Assert.Equal($"Recipe {id} not found", exception.Message);
    }

    #endregion

    #region GetRecentlyAddedRecipes

    [Fact]
    public async Task GetRecentlyAddedRecipes_ReturnsRecipesInReverseChronologicalOrder()
    {
        var allRecipes = new List<Recipe>();

        for (var i = 0; i < 15; i++)
        {
            allRecipes.Add(this.modelFactory.BuildRecipe() with
            {
                Created = new DateTime(2010, 1, 2, 3, 4, 5).AddHours(36 * i),
            });
        }

        using (var dbContext = this.databaseFixture.CreateDbContext())
        {
            dbContext.Recipes.AddRange(allRecipes);
            await dbContext.SaveChangesAsync();
        }

        var expected = allRecipes.AsEnumerable().Reverse().Take(10);

        var actual = await this.RecipeManager.GetRecentlyAddedRecipes();

        Assert.Equal(expected, actual);
    }

    #endregion

    #region GetRecentlyUpdatedRecipes

    [Fact]
    public async Task GetRecentlyUpdatedRecipes_ReturnsRecipesInReverseChronologicalOrder()
    {
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

        using (var dbContext = this.databaseFixture.CreateDbContext())
        {
            dbContext.Recipes.AddRange(allRecipes);
            await dbContext.SaveChangesAsync();
        }

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

        var actual = await this.RecipeManager.GetRecentlyUpdatedRecipes(
            new[] { allRecipes[3].Id, allRecipes[8].Id });

        Assert.Equal(expected, actual);
    }

    #endregion

    #region UpdateRecipe

    [Fact]
    public async Task UpdateRecipe_UpdatesAllUpdatableAttributes()
    {
        var original = this.modelFactory.BuildRecipe(setOptionalAttributes: true);
        var currentUser = this.modelFactory.BuildUser();

        using (var dbContext = this.databaseFixture.CreateDbContext())
        {
            dbContext.AddRange(original, currentUser);
            await dbContext.SaveChangesAsync();
        }

        var newAttributes = new RecipeAttributes(
            this.modelFactory.BuildRecipe(setOptionalAttributes: true));

        await this.RecipeManager.UpdateRecipe(
            original.Id, newAttributes, original.Revision, currentUser.Id);

        using (var dbContext = this.databaseFixture.CreateDbContext())
        {
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
    }

    [Fact]
    public async Task UpdateRecipe_AcceptsNullForOptionalAttributes()
    {
        var original = this.modelFactory.BuildRecipe(setOptionalAttributes: true);
        var currentUser = this.modelFactory.BuildUser();

        using (var dbContext = this.databaseFixture.CreateDbContext())
        {
            dbContext.AddRange(original, currentUser);
            await dbContext.SaveChangesAsync();
        }

        var newAttributes = new RecipeAttributes(
            this.modelFactory.BuildRecipe(setOptionalAttributes: false));

        await this.RecipeManager.UpdateRecipe(
            original.Id, newAttributes, original.Revision, currentUser.Id);

        using (var dbContext = this.databaseFixture.CreateDbContext())
        {
            var actual = await dbContext.Recipes.FindAsync(original.Id);

            Assert.NotNull(actual);
            Assert.Null(actual.PreparationMinutes);
            Assert.Null(actual.CookingMinutes);
            Assert.Null(actual.Servings);
            Assert.Null(actual.Suggestions);
            Assert.Null(actual.Remarks);
            Assert.Null(actual.Source);
        }
    }

    [Fact]
    public async Task UpdateRecipe_ThrowsIfRecordNotFound()
    {
        var otherRecipe = this.modelFactory.BuildRecipe();
        var currentUser = this.modelFactory.BuildUser();

        using (var dbContext = this.databaseFixture.CreateDbContext())
        {
            dbContext.AddRange(otherRecipe, currentUser);
            await dbContext.SaveChangesAsync();
        }

        var id = this.modelFactory.NextInt();

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => this.RecipeManager.UpdateRecipe(
                id, new(this.modelFactory.BuildRecipe()), 0, currentUser.Id));

        Assert.Equal($"Recipe {id} not found", exception.Message);
    }

    [Fact]
    public async Task UpdateRecipe_ThrowsIfRevisionOutOfSync()
    {
        var recipe = this.modelFactory.BuildRecipe(setOptionalAttributes: true);
        var currentUser = this.modelFactory.BuildUser();

        using (var dbContext = this.databaseFixture.CreateDbContext())
        {
            dbContext.AddRange(recipe, currentUser);
            await dbContext.SaveChangesAsync();
        }

        var staleRevision = recipe.Revision - 1;

        var exception = await Assert.ThrowsAsync<ConcurrencyException>(
            () => this.RecipeManager.UpdateRecipe(
                recipe.Id, new(this.modelFactory.BuildRecipe()), staleRevision, currentUser.Id));

        Assert.Equal(
            $"Revision {staleRevision} does not match current revision {recipe.Revision}",
            exception.Message);
    }

    #endregion
}
