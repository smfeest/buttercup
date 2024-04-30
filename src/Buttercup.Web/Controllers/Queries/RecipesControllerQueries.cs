using Buttercup.EntityModel;
using Microsoft.EntityFrameworkCore;

namespace Buttercup.Web.Controllers.Queries;

public sealed class RecipesControllerQueries : IRecipesControllerQueries
{
    public Task<Recipe?> FindRecipe(AppDbContext dbContext, long id) =>
        dbContext.Recipes.WhereNotSoftDeleted().FindAsync(id);

    public Task<Recipe?> FindRecipeForShowView(AppDbContext dbContext, long id) =>
        dbContext.Recipes
            .WhereNotSoftDeleted()
            .Include(r => r.CreatedByUser)
            .Include(r => r.ModifiedByUser)
            .FindAsync(id);

    public Task<Recipe[]> GetRecipesForIndex(AppDbContext dbContext) =>
        dbContext.Recipes.WhereNotSoftDeleted().OrderBy(r => r.Title).ToArrayAsync();
}