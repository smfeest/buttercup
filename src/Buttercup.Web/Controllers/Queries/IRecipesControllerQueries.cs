using Buttercup.EntityModel;

namespace Buttercup.Web.Controllers.Queries;

/// <summary>
/// Provides database queries for <see cref="RecipesController"/>.
/// </summary>
public interface IRecipesControllerQueries
{
    /// <summary>
    /// Finds a non-deleted a recipe.
    /// </summary>
    /// <param name="id">
    /// The recipe ID.
    /// </param>
    /// <param name="includeCreatedAndModifiedByUser">
    /// <b>true</b> to populate <see cref="Recipe.CreatedByUser"/> and  <see
    /// cref="Recipe.ModifiedByUser"/>, <b>false</b> otherwise.
    /// </param>
    /// <returns>
    /// A task for the operation. The result is the recipe, or null if the recipe does not exist or
    /// is soft-deleted.
    /// </returns>
    Task<Recipe?> FindRecipe(long id, bool includeCreatedAndModifiedByUser = false);

    /// <summary>
    /// Gets all non-deleted recipes ordered by title.
    /// </summary>
    /// <returns>
    /// A task for the operation.
    /// </returns>
    Task<IList<Recipe>> GetRecipes();
}
