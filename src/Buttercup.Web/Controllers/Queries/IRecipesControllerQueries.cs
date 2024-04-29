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
    /// <returns>
    /// A task for the operation. The result is the recipe, or null if the recipe does not exist or
    /// is soft-deleted.
    /// </returns>
    Task<Recipe?> FindRecipe(long id);

    /// <summary>
    /// Finds a non-deleted a recipe, including all of the associated records needed by the recipe
    /// `Show` view.
    /// </summary>
    /// <param name="id">
    /// The recipe ID.
    /// </param>
    /// <returns>
    /// A task for the operation. The result is the recipe, or null if the recipe does not exist or
    /// is soft-deleted.
    /// </returns>
    Task<Recipe?> FindRecipeForShowView(long id);

    /// <summary>
    /// Gets all non-deleted recipes ordered by title.
    /// </summary>
    /// <returns>
    /// A task for the operation.
    /// </returns>
    Task<IList<Recipe>> GetRecipesForIndex();
}
