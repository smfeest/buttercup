using Buttercup.EntityModel;
using Buttercup.Models;
using Microsoft.EntityFrameworkCore;

namespace Buttercup.DataAccess;

internal sealed class RecipeDataProvider : IRecipeDataProvider
{
    private readonly IClock clock;

    public RecipeDataProvider(IClock clock) => this.clock = clock;

    public async Task<long> AddRecipe(
        AppDbContext dbContext, RecipeAttributes attributes, long currentUserId)
    {
        var timestamp = this.clock.UtcNow;

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

        dbContext.Recipes.Add(recipe);

        await dbContext.SaveChangesAsync();

        return recipe.Id;
    }

    public async Task DeleteRecipe(AppDbContext dbContext, long id)
    {
        if (await dbContext.Recipes.Where(r => r.Id == id).ExecuteDeleteAsync() == 0)
        {
            throw RecipeNotFound(id);
        }
    }

    public async Task<IList<Recipe>> GetAllRecipes(AppDbContext dbContext) =>
        await dbContext.Recipes.OrderBy(r => r.Title).ToArrayAsync();

    public async Task<Recipe> GetRecipe(AppDbContext dbContext, long id) =>
        await dbContext.Recipes.FindAsync(id) ?? throw RecipeNotFound(id);

    public async Task<IList<Recipe>> GetRecentlyAddedRecipes(AppDbContext dbContext) =>
        await dbContext.Recipes.OrderByDescending(r => r.Created).Take(10).ToArrayAsync();

    public async Task<IList<Recipe>> GetRecentlyUpdatedRecipes(
        AppDbContext dbContext, IReadOnlyCollection<long> excludeRecipeIds) =>
        await dbContext
            .Recipes
            .Where(r => r.Created != r.Modified && !excludeRecipeIds.Contains(r.Id))
            .OrderByDescending(r => r.Modified)
            .Take(10)
            .ToArrayAsync();

    public async Task<IList<Recipe>> GetRecipes(
        AppDbContext dbContext, IReadOnlyCollection<long> ids) =>
        await dbContext.Recipes.Where(r => ids.Contains(r.Id)).ToArrayAsync();

    public async Task UpdateRecipe(
        AppDbContext dbContext,
        long id,
        RecipeAttributes newAttributes,
        int baseRevision,
        long currentUserId)
    {
        var updatedRows = await dbContext
            .Recipes
            .Where(r => r.Id == id && r.Revision == baseRevision)
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
                .SetProperty(r => r.Modified, this.clock.UtcNow)
                .SetProperty(r => r.ModifiedByUserId, currentUserId)
                .SetProperty(r => r.Revision, baseRevision + 1));

        if (updatedRows == 0)
        {
            throw await ConcurrencyOrNotFoundException(dbContext, id, baseRevision);
        }
    }

    private static async Task<Exception> ConcurrencyOrNotFoundException(
        AppDbContext dbContext, long id, int revision)
    {
        var currentRevision = await dbContext
            .Recipes
            .Where(r => r.Id == id)
            .Select<Recipe, long?>(r => r.Revision)
            .SingleOrDefaultAsync();

        return currentRevision == null ?
            RecipeNotFound(id) :
            new ConcurrencyException(
                $"Revision {revision} does not match current revision {currentRevision}");
    }

    private static NotFoundException RecipeNotFound(long id) => new($"Recipe {id} not found");
}
