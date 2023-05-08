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
        var recipe = await dbContext.Recipes.AsTracking().SingleAsync(r => r.Id == id);

        dbContext.Entry(recipe).Property(r => r.Revision).OriginalValue = baseRevision;

        recipe.Title = newAttributes.Title;
        recipe.PreparationMinutes = newAttributes.PreparationMinutes;
        recipe.CookingMinutes = newAttributes.CookingMinutes;
        recipe.Servings = newAttributes.Servings;
        recipe.Ingredients = newAttributes.Ingredients;
        recipe.Method = newAttributes.Method;
        recipe.Suggestions = newAttributes.Suggestions;
        recipe.Remarks = newAttributes.Remarks;
        recipe.Source = newAttributes.Source;
        recipe.Modified = this.clock.UtcNow;
        recipe.ModifiedByUserId = currentUserId;
        recipe.Revision = baseRevision + 1;

        await dbContext.SaveChangesAsync();
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
