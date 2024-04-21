using Buttercup.EntityModel;

namespace Buttercup.Web.Controllers.Queries;

/// <summary>
/// Provides database queries for <see cref="HomeController"/>.
/// </summary>
public interface IHomeControllerQueries
{
    /// <summary>
    /// Gets the ten most recently added recipes.
    /// </summary>
    /// <returns>
    /// A task for the operation.
    /// </returns>
    Task<IList<Recipe>> GetRecentlyAddedRecipes();

    /// <summary>
    /// Gets the ten most recently updated recipes.
    /// </summary>
    /// <remarks>
    /// Recipes that haven't been updated since they were added, and those with the IDs specified in
    /// <paramref name="excludeRecipeIds" />, are excluded from this list.
    /// </remarks>
    /// <param name="excludeRecipeIds">
    /// The IDs of the recipes that should be excluded.
    /// </param>
    /// <returns>
    /// A task for the operation.
    /// </returns>
    Task<IList<Recipe>> GetRecentlyUpdatedRecipes(IReadOnlyCollection<long> excludeRecipeIds);
}
