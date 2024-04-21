using Buttercup.EntityModel;
using Microsoft.EntityFrameworkCore;

namespace Buttercup.Web.Controllers.Queries;

public sealed class RecipesControllerQueries(IDbContextFactory<AppDbContext> dbContextFactory)
    : IRecipesControllerQueries
{
    private readonly IDbContextFactory<AppDbContext> dbContextFactory = dbContextFactory;

    public async Task<Recipe?> FindRecipe(
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

    public async Task<IList<Recipe>> GetRecipes()
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        return await dbContext.Recipes.WhereNotSoftDeleted().OrderBy(r => r.Title).ToArrayAsync();
    }
}
