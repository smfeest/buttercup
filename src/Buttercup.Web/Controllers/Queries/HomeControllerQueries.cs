using Buttercup.EntityModel;
using Microsoft.EntityFrameworkCore;

namespace Buttercup.Web.Controllers.Queries;

public sealed class HomeControllerQueries : IHomeControllerQueries
{
    public Task<Recipe[]> GetRecentlyAddedRecipes(AppDbContext dbContext) =>
        dbContext.Recipes
            .WhereNotSoftDeleted()
            .OrderByDescending(r => r.Created)
            .Take(10)
            .ToArrayAsync();

    public Task<Recipe[]> GetRecentlyUpdatedRecipes(
        AppDbContext dbContext, IReadOnlyCollection<long> excludeRecipeIds) =>
        dbContext.Recipes
            .WhereNotSoftDeleted()
            .Where(r => r.Created != r.Modified && !excludeRecipeIds.Contains(r.Id))
            .OrderByDescending(r => r.Modified)
            .Take(10)
            .ToArrayAsync();
}
