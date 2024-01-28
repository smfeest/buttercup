using Buttercup.EntityModel;

namespace Buttercup.Web.Api;

public sealed class CreateRecipePayload(long recipeId)
{
    /// <summary>
    /// The recipe.
    /// </summary>
    [UseSingleOrDefault]
    [UseProjection]
    public IQueryable<Recipe> Recipe(AppDbContext dbContext) =>
        dbContext.Recipes.Where(r => r.Id == recipeId);
}
