using Buttercup.EntityModel;

namespace Buttercup.Web.Api;

public sealed class UpdateRecipePayload(long recipeId)
{
    /// <summary>
    /// The updated recipe.
    /// </summary>
    [UseSingleOrDefault]
    [UseProjection]
    public IQueryable<Recipe> Recipe(AppDbContext dbContext) =>
        dbContext.Recipes.Where(r => r.Id == recipeId);
}
