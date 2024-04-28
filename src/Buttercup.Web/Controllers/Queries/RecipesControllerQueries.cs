using Buttercup.EntityModel;
using Microsoft.EntityFrameworkCore;

namespace Buttercup.Web.Controllers.Queries;

public sealed class RecipesControllerQueries(IDbContextFactory<AppDbContext> dbContextFactory)
    : IRecipesControllerQueries
{
    private readonly IDbContextFactory<AppDbContext> dbContextFactory = dbContextFactory;

    public async Task<Recipe?> FindRecipe(long id)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        return await dbContext.Recipes.WhereNotSoftDeleted().FindAsync(id);
    }

    public async Task<Recipe?> FindRecipeForShowView(long id)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        return await dbContext.Recipes
            .WhereNotSoftDeleted()
            .Include(r => r.CreatedByUser)
            .Include(r => r.ModifiedByUser)
            .FindAsync(id);
    }

    public async Task<IList<Recipe>> GetRecipes()
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        return await dbContext.Recipes.WhereNotSoftDeleted().OrderBy(r => r.Title).ToArrayAsync();
    }
}
