using Buttercup.EntityModel;

namespace Buttercup.Web.Api;

public sealed class DeleteRecipePayload(long recipeId, bool deleted)
{
    /// <summary>
    /// <b>true</b> if the recipe was soft-deleted; <b>false</b> if the recipe does not exist or has
    /// already been soft-deleted.
    /// </summary>
    public bool Deleted => deleted;

    /// <summary>
    /// The deleted recipe.
    /// </summary>
    [UseSingleOrDefault]
    [UseProjection]
    public IQueryable<Recipe> Recipe(AppDbContext dbContext) =>
        dbContext.Recipes.Where(r => r.Id == recipeId).OrderBy(r => r.Id);
}
