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

        using var dbContext = this.dbContextFactory.CreateDbContext();
        dbContext.Recipes.Add(recipe);
        await dbContext.SaveChangesAsync();

        return recipe.Id;
    }

    public async Task DeleteRecipe(long id)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        if (await dbContext.Recipes.Where(r => r.Id == id).ExecuteDeleteAsync() == 0)
        {
            throw RecipeNotFound(id);
        }
    }

    public async Task<IList<Recipe>> GetNonDeletedRecipes()
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        return await dbContext.Recipes.WhereNotSoftDeleted().OrderBy(r => r.Title).ToArrayAsync();
    }

    public async Task<Recipe> GetRecipe(long id, bool includeCreatedAndModifiedByUser = false)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        IQueryable<Recipe> queryable = dbContext.Recipes;

        if (includeCreatedAndModifiedByUser)
        {
            queryable = queryable.Include(r => r.CreatedByUser).Include(r => r.ModifiedByUser);
        }

        return await queryable.GetAsync(id);
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
            .Where(r => r.Created != r.Modified && !excludeRecipeIds.Contains(r.Id))
            .OrderByDescending(r => r.Modified)
            .Take(10)
            .ToArrayAsync();
    }

    public async Task UpdateRecipe(
        long id, RecipeAttributes newAttributes, int baseRevision, long currentUserId)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        var updatedRows = await dbContext
            .Recipes
            .Where(r => r.Id == id && !r.Deleted.HasValue && r.Revision == baseRevision)
            .ExecuteUpdateAsync(s => s
                .SetProperty(r => r.Title, newAttributes.Title)
                .SetProperty(r => r.PreparationMinutes, newAttributes.PreparationMinutes)
                .SetProperty(r => r.CookingMinutes, newAttributes.CookingMinutes)
                .SetProperty(r => r.Servings, newAttributes.Servings)
                .SetProperty(r => r.Ingredients, newAttributes.Ingredients)
                .SetProperty(r => r.Method, newAttributes.Method)
                .SetProperty(r => r.Suggestions, newAttributes.Suggestions)
                .SetProperty(r => r.Remarks, newAttributes.Remarks)
                .SetProperty(r => r.Source, newAttributes.Source)
                .SetProperty(r => r.Modified, this.timeProvider.GetUtcDateTimeNow())
                .SetProperty(r => r.ModifiedByUserId, currentUserId)
                .SetProperty(r => r.Revision, baseRevision + 1));

        if (updatedRows == 0)
        {
            var recipe = await dbContext.Recipes.GetAsync(id);

            throw recipe.Deleted.HasValue ?
                new SoftDeletedException($"Cannot update soft-deleted recipe {id}") :
                new ConcurrencyException(
                    $"Revision {baseRevision} does not match current revision {recipe.Revision}");
        }
    }

    private static NotFoundException RecipeNotFound(long id) => new($"Recipe {id} not found");
}
