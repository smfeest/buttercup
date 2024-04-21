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
