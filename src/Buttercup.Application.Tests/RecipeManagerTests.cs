using Buttercup.EntityModel;
using Buttercup.TestUtils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace Buttercup.Application;

[Collection(nameof(DatabaseCollection))]
public sealed class RecipeManagerTests : DatabaseTests<DatabaseCollection>
{
    private readonly ModelFactory modelFactory = new();

    private readonly FakeTimeProvider timeProvider;
    private readonly RecipeManager recipeManager;

    public RecipeManagerTests(DatabaseFixture<DatabaseCollection> databaseFixture)
        : base(databaseFixture)
    {
        this.timeProvider = new(this.modelFactory.NextDateTime());
        this.recipeManager = new(databaseFixture, this.timeProvider);
    }

    #region AddRecipe

    [Fact]
    public async Task AddRecipe_InsertsRecipeAndReturnsId()
    {
        var currentUser = this.modelFactory.BuildUser();
        var attributes = new RecipeAttributes(
            this.modelFactory.BuildRecipe(setOptionalAttributes: true));

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            dbContext.Users.Add(currentUser);
            await dbContext.SaveChangesAsync();
        }

        var id = await this.recipeManager.AddRecipe(attributes, currentUser.Id);

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
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
                Created = this.timeProvider.GetUtcDateTimeNow(),
                CreatedByUserId = currentUser.Id,
                Modified = this.timeProvider.GetUtcDateTimeNow(),
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

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            dbContext.Users.Add(currentUser);
            await dbContext.SaveChangesAsync();
        }

        var id = await this.recipeManager.AddRecipe(attributes, currentUser.Id);

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
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
    public async Task DeleteRecipe_SetsSoftDeleteAttributesAndReturnsTrue()
    {
        var original = this.modelFactory.BuildRecipe(softDeleted: false);
        var currentUser = this.modelFactory.BuildUser();

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            dbContext.AddRange(original, currentUser);
            await dbContext.SaveChangesAsync();
        }

        Assert.True(await this.recipeManager.DeleteRecipe(original.Id, currentUser.Id));

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            var expected = original with
            {
                Deleted = this.timeProvider.GetUtcDateTimeNow(),
                DeletedByUserId = currentUser.Id,
            };
            var actual = await dbContext.Recipes.FindAsync(original.Id);
            Assert.Equal(expected, actual);
        }
    }

    [Fact]
    public async Task DeleteRecipe_DoesNotUpdateAttributesAndReturnsFalseIfAlreadySoftDeleted()
    {
        var original = this.modelFactory.BuildRecipe(softDeleted: true);
        var currentUser = this.modelFactory.BuildUser();

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            dbContext.AddRange(original, currentUser);
            await dbContext.SaveChangesAsync();
        }

        Assert.False(await this.recipeManager.DeleteRecipe(original.Id, currentUser.Id));

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            var actual = await dbContext.Recipes.FindAsync(original.Id);
            Assert.Equal(original, actual);
        }
    }

    [Fact]
    public async Task DeleteRecipe_ReturnsFalseIfRecordNotFound()
    {
        var currentUser = this.modelFactory.BuildUser();

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            dbContext.AddRange(this.modelFactory.BuildRecipe(), currentUser);
            await dbContext.SaveChangesAsync();
        }

        Assert.False(
            await this.recipeManager.DeleteRecipe(this.modelFactory.NextInt(), currentUser.Id));
    }

    #endregion

    #region FindNonDeletedRecipe

    [Fact]
    public async Task FindNonDeletedRecipe_ReturnsRecipeWithCreatedAndModifiedByUser()
    {
        var expected = this.modelFactory.BuildRecipe(setOptionalAttributes: true);

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            dbContext.Recipes.Add(expected);
            await dbContext.SaveChangesAsync();
        }

        var actual = await this.recipeManager.FindNonDeletedRecipe(expected.Id, true);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task FindNonDeletedRecipe_ReturnsRecipeWithoutCreatedAndModifiedByUser()
    {
        var recipe = this.modelFactory.BuildRecipe(setOptionalAttributes: true);

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            dbContext.Recipes.Add(recipe);
            await dbContext.SaveChangesAsync();
        }

        var actual = await this.recipeManager.FindNonDeletedRecipe(recipe.Id, false);
        var expected = recipe with
        {
            CreatedByUser = null,
            ModifiedByUser = null,
        };

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task FindNonDeletedRecipe_ReturnsNullIfRecordNotFound()
    {
        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            dbContext.Recipes.Add(this.modelFactory.BuildRecipe());
            await dbContext.SaveChangesAsync();
        }

        var id = this.modelFactory.NextInt();
        Assert.Null(await this.recipeManager.FindNonDeletedRecipe(id));
    }

    #endregion

    #region GetNonDeletedRecipes

    [Fact]
    public async Task GetNonDeletedRecipes_ReturnsNonDeletedRecipesInTitleOrder()
    {
        var recipeB = this.modelFactory.BuildRecipe() with { Title = "recipe-title-b" };
        var recipeC = this.modelFactory.BuildRecipe() with { Title = "recipe-title-c" };
        var recipeA = this.modelFactory.BuildRecipe() with { Title = "recipe-title-a" };
        var deletedRecipe = this.modelFactory.BuildRecipe(softDeleted: true);

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            dbContext.Recipes.AddRange(recipeB, recipeC, recipeA, deletedRecipe);
            await dbContext.SaveChangesAsync();
        }

        var expected = new Recipe[] {
            recipeA,
            recipeB,
            recipeC,
        };

        var actual = await this.recipeManager.GetNonDeletedRecipes();

        Assert.Equal(expected, actual);
    }

    #endregion

    #region GetRecentlyAddedRecipes

    [Fact]
    public async Task GetRecentlyAddedRecipes_ReturnsNonDeletedRecipesInReverseChronologicalOrder()
    {
        var allRecipes = new List<Recipe>();

        for (var i = 0; i < 20; i++)
        {
            allRecipes.Add(this.modelFactory.BuildRecipe(softDeleted: i % 5 == 0) with
            {
                Created = new DateTime(2010, 1, 2, 3, 4, 5).AddHours(36 * i),
            });
        }

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            dbContext.Recipes.AddRange(allRecipes);
            await dbContext.SaveChangesAsync();
        }

        var expected = allRecipes.AsEnumerable().Where(r => !r.Deleted.HasValue).Reverse().Take(10);

        var actual = await this.recipeManager.GetRecentlyAddedRecipes();

        Assert.Equal(expected, actual);
    }

    #endregion

    #region GetRecentlyUpdatedRecipes

    [Fact]
    public async Task GetRecentlyUpdatedRecipes_ReturnsRecipesInReverseChronologicalOrder()
    {
        var baseDateTime = this.modelFactory.NextDateTime();

        Recipe BuildRecipe(int createdDaysAgo, int modifiedDaysAgo, bool softDeleted = false) =>
            this.modelFactory.BuildRecipe(softDeleted: softDeleted) with
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
            BuildRecipe(1, 2, true), // soft-deleted
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

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            dbContext.Recipes.AddRange(allRecipes);
            await dbContext.SaveChangesAsync();
        }

        var expected = new[]
        {
            allRecipes[2],
            allRecipes[9],
            allRecipes[12],
            allRecipes[14],
            allRecipes[13],
            allRecipes[1],
            allRecipes[0],
            allRecipes[4],
            allRecipes[7],
            allRecipes[15],
        };

        var actual = await this.recipeManager.GetRecentlyUpdatedRecipes(
            [allRecipes[3].Id, allRecipes[8].Id]);

        Assert.Equal(expected, actual);
    }

    #endregion

    #region HardDeleteRecipe

    [Fact]
    public async Task HardDeleteRecipe_HardDeletesRecipeAndReturnsTrue()
    {
        var recipe = this.modelFactory.BuildRecipe();

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            dbContext.Recipes.Add(recipe);
            await dbContext.SaveChangesAsync();
        }

        Assert.True(await this.recipeManager.HardDeleteRecipe(recipe.Id));

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            Assert.False(await dbContext.Recipes.AnyAsync());
        }
    }

    [Fact]
    public async Task HardDeleteRecipe_ReturnsFalseIfRecordNotFound()
    {
        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            dbContext.Recipes.Add(this.modelFactory.BuildRecipe());
            await dbContext.SaveChangesAsync();
        }

        var id = this.modelFactory.NextInt();

        Assert.False(await this.recipeManager.HardDeleteRecipe(id));
    }

    #endregion

    #region UpdateRecipe

    [Fact]
    public async Task UpdateRecipe_UpdatesAllUpdatableAttributes()
    {
        var original = this.modelFactory.BuildRecipe(setOptionalAttributes: true);
        var currentUser = this.modelFactory.BuildUser();

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            dbContext.AddRange(original, currentUser);
            await dbContext.SaveChangesAsync();
        }

        var newAttributes = new RecipeAttributes(
            this.modelFactory.BuildRecipe(setOptionalAttributes: true));

        Assert.True(await this.recipeManager.UpdateRecipe(
            original.Id, newAttributes, original.Revision, currentUser.Id));

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
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
                Modified = this.timeProvider.GetUtcDateTimeNow(),
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

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            dbContext.AddRange(original, currentUser);
            await dbContext.SaveChangesAsync();
        }

        var newAttributes = new RecipeAttributes(
            this.modelFactory.BuildRecipe(setOptionalAttributes: false));

        Assert.True(await this.recipeManager.UpdateRecipe(
            original.Id, newAttributes, original.Revision, currentUser.Id));

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
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
    public async Task UpdateRecipe_ReturnsFalseAndDoesNotUpdateIfAttributesAlreadyMatch()
    {
        var original = this.modelFactory.BuildRecipe(setOptionalAttributes: true);
        var currentUser = this.modelFactory.BuildUser();

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            dbContext.AddRange(original, currentUser);
            await dbContext.SaveChangesAsync();
        }

        Assert.False(await this.recipeManager.UpdateRecipe(
            original.Id, new(original), original.Revision, currentUser.Id));

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            var expected = original with { CreatedByUser = null, ModifiedByUser = null };
            var actual = await dbContext.Recipes.FindAsync(original.Id);

            Assert.Equal(expected, actual);
        }
    }

    [Fact]
    public async Task UpdateRecipe_ThrowsIfRecordNotFound()
    {
        var otherRecipe = this.modelFactory.BuildRecipe();
        var currentUser = this.modelFactory.BuildUser();

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            dbContext.AddRange(otherRecipe, currentUser);
            await dbContext.SaveChangesAsync();
        }

        var id = this.modelFactory.NextInt();

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => this.recipeManager.UpdateRecipe(
                id, new(this.modelFactory.BuildRecipe()), 0, currentUser.Id));

        Assert.Equal($"Recipe/{id} not found", exception.Message);
    }

    [Fact]
    public async Task UpdateRecipe_ThrowsIfRecipeSoftDeleted()
    {
        var recipe = this.modelFactory.BuildRecipe(softDeleted: true);
        var currentUser = this.modelFactory.BuildUser();

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            dbContext.AddRange(recipe, currentUser);
            await dbContext.SaveChangesAsync();
        }

        var exception = await Assert.ThrowsAsync<SoftDeletedException>(
            () => this.recipeManager.UpdateRecipe(
                recipe.Id, new(this.modelFactory.BuildRecipe()), recipe.Revision, currentUser.Id));

        Assert.Equal($"Cannot update soft-deleted recipe {recipe.Id}", exception.Message);
    }

    [Fact]
    public async Task UpdateRecipe_ThrowsIfRevisionOutOfSync()
    {
        var recipe = this.modelFactory.BuildRecipe(setOptionalAttributes: true);
        var currentUser = this.modelFactory.BuildUser();

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            dbContext.AddRange(recipe, currentUser);
            await dbContext.SaveChangesAsync();
        }

        var staleRevision = recipe.Revision - 1;

        var exception = await Assert.ThrowsAsync<ConcurrencyException>(
            () => this.recipeManager.UpdateRecipe(
                recipe.Id, new(this.modelFactory.BuildRecipe()), staleRevision, currentUser.Id));

        Assert.Equal(
            $"Revision {staleRevision} does not match current revision {recipe.Revision}",
            exception.Message);
    }

    #endregion
}
