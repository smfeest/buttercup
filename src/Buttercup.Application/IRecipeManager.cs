using Buttercup.EntityModel;

namespace Buttercup.Application;

/// <summary>
/// Defines the contract for the recipe data provider.
/// </summary>
public interface IRecipeManager
{
    /// <summary>
    /// Adds a new recipe.
    /// </summary>
    /// <param name="attributes">
    /// The recipe attributes.
    /// </param>
    /// <param name="currentUserId">
    /// The current user ID.
    /// </param>
    /// <returns>
    /// A task for the operation. The task result is the ID of the new recipe.
    /// </returns>
    Task<long> AddRecipe(RecipeAttributes attributes, long currentUserId);

    /// <summary>
    /// Soft-deletes a recipe.
    /// </summary>
    /// <param name="id">
    /// The recipe ID.
    /// </param>
    /// <param name="currentUserId">
    /// The current user ID.
    /// </param>
    /// <returns>
    /// A task for the operation. The task result is <b>true</b> on success, <b>false</b> if the
    /// recipe does not exist or has already been soft-deleted.
    /// </returns>
    Task<bool> DeleteRecipe(long id, long currentUserId);

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
    Task<Recipe?> FindNonDeletedRecipe(long id, bool includeCreatedAndModifiedByUser = false);

    /// <summary>
    /// Gets all non-deleted recipes ordered by title.
    /// </summary>
    /// <returns>
    /// A task for the operation.
    /// </returns>
    Task<IList<Recipe>> GetNonDeletedRecipes();

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

    /// <summary>
    /// Hard-deletes a recipe.
    /// </summary>
    /// <param name="id">
    /// The recipe ID.
    /// </param>
    /// <returns>
    /// A task for the operation. The task result is <b>true</b> on success, <b>false</b> if the
    /// recipe does not exist.
    /// </returns>
    Task<bool> HardDeleteRecipe(long id);

    /// <summary>
    /// Updates a recipe.
    /// </summary>
    /// <param name="id">
    /// The recipe ID.
    /// </param>
    /// <param name="newAttributes">
    /// The new recipe attributes.
    /// </param>
    /// <param name="baseRevision">
    /// The base revision.
    /// </param>
    /// <param name="currentUserId">
    /// The current user ID.
    /// </param>
    /// <returns>
    /// A task for the operation. The task result is <b>true</b> if the recipe was updated,
    /// <b>false</b> if the recipe's attributes already matched <paramref name="newAttributes"/>.
    /// </returns>
    /// <exception cref="NotFoundException">
    /// No matching recipe was found.
    /// </exception>
    /// <exception cref="SoftDeletedException">
    /// Recipe is soft-deleted.
    /// </exception>
    /// <exception cref="ConcurrencyException">
    /// <paramref name="baseRevision"/> does not match the current revision in the database.
    /// </exception>
    Task<bool> UpdateRecipe(
        long id, RecipeAttributes newAttributes, int baseRevision, long currentUserId);
}
