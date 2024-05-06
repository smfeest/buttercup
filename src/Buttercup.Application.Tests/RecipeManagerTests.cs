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
    public async Task AddRecipe_InsertsRecipeAndRevisionAndReturnsId()
    {
        var currentUser = this.modelFactory.BuildUser();
        var attributes = new RecipeAttributes(
            this.modelFactory.BuildRecipe(setOptionalAttributes: true));
        await this.DatabaseFixture.InsertEntities(currentUser);

        var id = await this.recipeManager.AddRecipe(attributes, currentUser.Id);

        using var dbContext = this.DatabaseFixture.CreateDbContext();

        var expectedRecipe = new Recipe
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
        var actualRecipe = await dbContext.Recipes.FindAsync(id);
        Assert.Equivalent(expectedRecipe, actualRecipe);

        var expectedRevision = new RecipeRevision
        {
            RecipeId = id,
            Revision = 0,
            Created = this.timeProvider.GetUtcDateTimeNow(),
            CreatedByUserId = currentUser.Id,
            Title = attributes.Title,
            PreparationMinutes = attributes.PreparationMinutes,
            CookingMinutes = attributes.CookingMinutes,
            Servings = attributes.Servings,
            Ingredients = attributes.Ingredients,
            Method = attributes.Method,
            Suggestions = attributes.Suggestions,
            Remarks = attributes.Remarks,
            Source = attributes.Source,
        };
        var actualRevision = await dbContext.RecipeRevisions.SingleAsync();
        Assert.Equivalent(expectedRevision, actualRevision);
    }

    [Fact]
    public async Task AddRecipe_AcceptsNullForOptionalAttributes()
    {
        var currentUser = this.modelFactory.BuildUser();
        var attributes = new RecipeAttributes(
            this.modelFactory.BuildRecipe(setOptionalAttributes: false));
        await this.DatabaseFixture.InsertEntities(currentUser);

        var id = await this.recipeManager.AddRecipe(attributes, currentUser.Id);

        using var dbContext = this.DatabaseFixture.CreateDbContext();

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
    public async Task DeleteRecipe_SetsSoftDeleteAttributesAndReturnsTrue()
    {
        var original = this.modelFactory.BuildRecipe(softDeleted: false);
        var currentUser = this.modelFactory.BuildUser();
        await this.DatabaseFixture.InsertEntities(original, currentUser);

        Assert.True(await this.recipeManager.DeleteRecipe(original.Id, currentUser.Id));

        using var dbContext = this.DatabaseFixture.CreateDbContext();

        var expected = original with
        {
            Deleted = this.timeProvider.GetUtcDateTimeNow(),
            DeletedByUserId = currentUser.Id,
        };
        var actual = await dbContext.Recipes.FindAsync(original.Id);
        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public async Task DeleteRecipe_DoesNotUpdateAttributesAndReturnsFalseIfAlreadySoftDeleted()
    {
        var original = this.modelFactory.BuildRecipe(softDeleted: true);
        var currentUser = this.modelFactory.BuildUser();
        await this.DatabaseFixture.InsertEntities(original, currentUser);

        Assert.False(await this.recipeManager.DeleteRecipe(original.Id, currentUser.Id));

        using var dbContext = this.DatabaseFixture.CreateDbContext();

        var actual = await dbContext.Recipes.FindAsync(original.Id);
        Assert.Equivalent(original, actual);
    }

    [Fact]
    public async Task DeleteRecipe_ReturnsFalseIfRecordNotFound()
    {
        var currentUser = this.modelFactory.BuildUser();
        await this.DatabaseFixture.InsertEntities(this.modelFactory.BuildRecipe(), currentUser);

        Assert.False(
            await this.recipeManager.DeleteRecipe(this.modelFactory.NextInt(), currentUser.Id));
    }

    #endregion

    #region HardDeleteRecipe

    [Fact]
    public async Task HardDeleteRecipe_HardDeletesRecipeAndReturnsTrue()
    {
        var recipe = this.modelFactory.BuildRecipe();
        await this.DatabaseFixture.InsertEntities(recipe);

        Assert.True(await this.recipeManager.HardDeleteRecipe(recipe.Id));

        using var dbContext = this.DatabaseFixture.CreateDbContext();

        Assert.False(await dbContext.Recipes.AnyAsync());
    }

    [Fact]
    public async Task HardDeleteRecipe_ReturnsFalseIfRecordNotFound()
    {
        await this.DatabaseFixture.InsertEntities(this.modelFactory.BuildRecipe());

        Assert.False(await this.recipeManager.HardDeleteRecipe(this.modelFactory.NextInt()));
    }

    #endregion

    #region UpdateRecipe

    [Fact]
    public async Task UpdateRecipe_UpdatesRecipeInsertsRevisionAndReturnsTrue()
    {
        var original = this.modelFactory.BuildRecipe(setOptionalAttributes: true);
        var currentUser = this.modelFactory.BuildUser();
        await this.DatabaseFixture.InsertEntities(original, currentUser);

        var newAttributes = new RecipeAttributes(
            this.modelFactory.BuildRecipe(setOptionalAttributes: true));

        Assert.True(await this.recipeManager.UpdateRecipe(
            original.Id, newAttributes, original.Revision, currentUser.Id));

        using var dbContext = this.DatabaseFixture.CreateDbContext();

        var expectedRecipe = new Recipe
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
        var actualRecipe = await dbContext.Recipes.FindAsync(original.Id);
        Assert.Equivalent(expectedRecipe, actualRecipe);

        var expectedRevision = new RecipeRevision
        {
            RecipeId = original.Id,
            Revision = original.Revision + 1,
            Created = this.timeProvider.GetUtcDateTimeNow(),
            CreatedByUserId = currentUser.Id,
            Title = newAttributes.Title,
            PreparationMinutes = newAttributes.PreparationMinutes,
            CookingMinutes = newAttributes.CookingMinutes,
            Servings = newAttributes.Servings,
            Ingredients = newAttributes.Ingredients,
            Method = newAttributes.Method,
            Suggestions = newAttributes.Suggestions,
            Remarks = newAttributes.Remarks,
            Source = newAttributes.Source,
        };
        var actualRevision = await dbContext.RecipeRevisions.SingleAsync();
        Assert.Equivalent(expectedRevision, actualRevision);
    }

    [Fact]
    public async Task UpdateRecipe_AcceptsNullForOptionalAttributes()
    {
        var original = this.modelFactory.BuildRecipe(setOptionalAttributes: true);
        var currentUser = this.modelFactory.BuildUser();
        await this.DatabaseFixture.InsertEntities(original, currentUser);

        var newAttributes = new RecipeAttributes(
            this.modelFactory.BuildRecipe(setOptionalAttributes: false));

        Assert.True(await this.recipeManager.UpdateRecipe(
            original.Id, newAttributes, original.Revision, currentUser.Id));

        using var dbContext = this.DatabaseFixture.CreateDbContext();
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
    public async Task UpdateRecipe_ReturnsFalseAndDoesNotUpdateIfAttributesAlreadyMatch()
    {
        var original = this.modelFactory.BuildRecipe(setOptionalAttributes: true);
        var currentUser = this.modelFactory.BuildUser();
        await this.DatabaseFixture.InsertEntities(original, currentUser);

        Assert.False(await this.recipeManager.UpdateRecipe(
            original.Id, new(original), original.Revision, currentUser.Id));

        using var dbContext = this.DatabaseFixture.CreateDbContext();
        var expected = original with { CreatedByUser = null, ModifiedByUser = null };
        var actual = await dbContext.Recipes.FindAsync(original.Id);

        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public async Task UpdateRecipe_ThrowsIfRecordNotFound()
    {
        var otherRecipe = this.modelFactory.BuildRecipe();
        var currentUser = this.modelFactory.BuildUser();
        await this.DatabaseFixture.InsertEntities(otherRecipe, currentUser);

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
        await this.DatabaseFixture.InsertEntities(recipe, currentUser);

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
        await this.DatabaseFixture.InsertEntities(recipe, currentUser);

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
