using Buttercup.EntityModel;
using Microsoft.EntityFrameworkCore;

namespace Buttercup.Application;

internal sealed class RecipeManager(
    IDbContextFactory<AppDbContext> dbContextFactory, TimeProvider timeProvider)
    : IRecipeManager
{
    private readonly TimeProvider timeProvider = timeProvider;
    private readonly IDbContextFactory<AppDbContext> dbContextFactory = dbContextFactory;

    public async Task<long> AddRecipe(RecipeAttributes attributes, long currentUserId)
    {
        var timestamp = this.timeProvider.GetUtcDateTimeNow();
        var recipe = new Recipe()
        {
            Title = attributes.Title,
            PreparationMinutes = attributes.PreparationMinutes,
            CookingMinutes = attributes.CookingMinutes,
            Servings = attributes.Servings,
            Ingredients = attributes.Ingredients,
            Method = attributes.Method,
            Suggestions = attributes.Suggestions,
            Remarks = attributes.Remarks,
            Source = attributes.Source,
            Created = timestamp,
            CreatedByUserId = currentUserId,
            Modified = timestamp,
            ModifiedByUserId = currentUserId
        };

        recipe.Revisions.Add(RecipeRevision.From(recipe));

        using var dbContext = this.dbContextFactory.CreateDbContext();
        dbContext.Recipes.Add(recipe);
        await dbContext.SaveChangesAsync();

        return recipe.Id;
    }

    public async Task<bool> DeleteRecipe(long id, long currentUserId)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        var updatedRows = await dbContext
            .Recipes
            .Where(r => r.Id == id)
            .WhereNotSoftDeleted()
            .ExecuteUpdateAsync(s => s
                .SetProperty(r => r.Deleted, this.timeProvider.GetUtcDateTimeNow())
                .SetProperty(r => r.DeletedByUserId, currentUserId));

        return updatedRows > 0;
    }

    public async Task<Recipe?> FindNonDeletedRecipe(
        long id, bool includeCreatedAndModifiedByUser = false)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        var queryable = dbContext.Recipes.WhereNotSoftDeleted();

        if (includeCreatedAndModifiedByUser)
        {
            queryable = queryable.Include(r => r.CreatedByUser).Include(r => r.ModifiedByUser);
        }

        return await queryable.FindAsync(id);
    }

    public async Task<IList<Recipe>> GetNonDeletedRecipes()
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        return await dbContext.Recipes.WhereNotSoftDeleted().OrderBy(r => r.Title).ToArrayAsync();
    }

    public async Task<IList<Recipe>> GetRecentlyAddedRecipes()
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        return await dbContext.Recipes
            .WhereNotSoftDeleted()
            .OrderByDescending(r => r.Created)
            .Take(10)
            .ToArrayAsync();
    }

    public async Task<IList<Recipe>> GetRecentlyUpdatedRecipes(
        IReadOnlyCollection<long> excludeRecipeIds)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        return await dbContext
            .Recipes
            .WhereNotSoftDeleted()
            .Where(r => r.Created != r.Modified && !excludeRecipeIds.Contains(r.Id))
            .OrderByDescending(r => r.Modified)
            .Take(10)
            .ToArrayAsync();
    }

    public async Task<bool> HardDeleteRecipe(long id)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        return await dbContext.Recipes.Where(r => r.Id == id).ExecuteDeleteAsync() != 0;
    }

    public async Task<bool> UpdateRecipe(
        long id, RecipeAttributes newAttributes, int baseRevision, long currentUserId)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        var recipe = await dbContext.Recipes.AsTracking().GetAsync(id);

        if (recipe.Deleted.HasValue)
        {
            throw new SoftDeletedException($"Cannot update soft-deleted recipe {id}");
        }
        if (newAttributes == new RecipeAttributes(recipe))
        {
            return false;
        }
        if (recipe.Revision != baseRevision)
        {
            throw new ConcurrencyException(
                $"Revision {baseRevision} does not match current revision {recipe.Revision}");
        }

        recipe.Title = newAttributes.Title;
        recipe.PreparationMinutes = newAttributes.PreparationMinutes;
        recipe.CookingMinutes = newAttributes.CookingMinutes;
        recipe.Servings = newAttributes.Servings;
        recipe.Ingredients = newAttributes.Ingredients;
        recipe.Method = newAttributes.Method;
        recipe.Suggestions = newAttributes.Suggestions;
        recipe.Remarks = newAttributes.Remarks;
        recipe.Source = newAttributes.Source;
        recipe.Modified = this.timeProvider.GetUtcDateTimeNow();
        recipe.ModifiedByUserId = currentUserId;
        recipe.Revision++;

        recipe.Revisions.Add(RecipeRevision.From(recipe));

        try
        {
            await dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return await this.UpdateRecipe(id, newAttributes, baseRevision, currentUserId);
        }

        return true;
    }
}
