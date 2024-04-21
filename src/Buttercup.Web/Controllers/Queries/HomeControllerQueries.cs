using Buttercup.EntityModel;
using Microsoft.EntityFrameworkCore;

namespace Buttercup.Web.Controllers.Queries;

public sealed class HomeControllerQueries(IDbContextFactory<AppDbContext> dbContextFactory)
    : IHomeControllerQueries
{
    private readonly IDbContextFactory<AppDbContext> dbContextFactory = dbContextFactory;

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
}
