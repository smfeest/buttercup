using Buttercup.EntityModel;
using Microsoft.EntityFrameworkCore;

namespace Buttercup.Web.Controllers.Queries;

public sealed class HomeControllerQueries : IHomeControllerQueries
{
    public Task<Recipe[]> GetRecentlyAddedRecipes(
        AppDbContext dbContext, CancellationToken cancellationToken) =>
        dbContext.Recipes
            .WhereNotSoftDeleted()
            .OrderByDescending(r => r.Created)
            .Take(10)
            .ToArrayAsync(cancellationToken);

    public Task<Recipe[]> GetRecentlyUpdatedRecipes(
        AppDbContext dbContext,
        IReadOnlyCollection<long> excludeRecipeIds,
        CancellationToken cancellationToken) =>
        dbContext.Recipes
            .WhereNotSoftDeleted()
            .Where(r => r.Created != r.Modified && !excludeRecipeIds.Contains(r.Id))
            .OrderByDescending(r => r.Modified)
            .Take(10)
            .ToArrayAsync(cancellationToken);
}
