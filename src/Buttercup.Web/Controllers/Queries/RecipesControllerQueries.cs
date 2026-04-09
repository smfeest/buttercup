using Buttercup.EntityModel;
using Microsoft.EntityFrameworkCore;

namespace Buttercup.Web.Controllers.Queries;

public sealed class RecipesControllerQueries : IRecipesControllerQueries
{
    public Task<Recipe?> FindRecipe(
        AppDbContext dbContext, long id, CancellationToken cancellationToken) =>
        dbContext.Recipes.WhereNotSoftDeleted().FindAsync(id, cancellationToken);

    public Task<Recipe?> FindRecipeForShowView(
        AppDbContext dbContext, long id, CancellationToken cancellationToken) =>
        dbContext.Recipes
            .WhereNotSoftDeleted()
            .Include(r => r.CreatedByUser)
            .Include(r => r.ModifiedByUser)
            .FindAsync(id, cancellationToken);

    public Task<Comment[]> GetCommentsForRecipe(
        AppDbContext dbContext, long recipeId, CancellationToken cancellationToken) =>
        dbContext.Comments
            .WhereNotSoftDeleted()
            .Where(c => c.RecipeId == recipeId)
            .Include(c => c.Author)
            .OrderBy(c => c.Id)
            .ToArrayAsync(cancellationToken);

    public Task<Recipe[]> GetRecipesForIndex(
        AppDbContext dbContext, CancellationToken cancellationToken) =>
        dbContext.Recipes
            .WhereNotSoftDeleted()
            .OrderBy(r => r.Title)
            .ToArrayAsync(cancellationToken);
}
