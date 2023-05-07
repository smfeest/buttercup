using Buttercup.EntityModel;
using Buttercup.Models;
using MySqlConnector;

namespace Buttercup.DataAccess;

/// <summary>
/// Defines the contract for the recipe data provider.
/// </summary>
public interface IRecipeDataProvider
{
    /// <summary>
    /// Adds a new recipe.
    /// </summary>
    /// <param name="connection">
    /// The database connection.
    /// </param>
    /// <param name="attributes">
    /// The recipe attributes.
    /// </param>
    /// <param name="currentUserId">
    /// The current user ID.
    /// </param>
    /// <returns>
    /// A task for the operation. The task result is the ID of the new recipe.
    /// </returns>
    Task<long> AddRecipe(
        MySqlConnection connection, RecipeAttributes attributes, long currentUserId);

    /// <summary>
    /// Deletes a recipe.
    /// </summary>
    /// <param name="connection">
    /// The database connection.
    /// </param>
    /// <param name="id">
    /// The recipe ID.
    /// </param>
    /// <returns>
    /// A task for the operation.
    /// </returns>
    /// <exception cref="NotFoundException">
    /// No matching recipe was found.
    /// </exception>
    Task DeleteRecipe(MySqlConnection connection, long id);

    /// <summary>
    /// Gets all the recipes ordered by title.
    /// </summary>
    /// <param name="connection">
    /// The database connection.
    /// </param>
    /// <returns>
    /// A task for the operation.
    /// </returns>
    Task<IList<Recipe>> GetAllRecipes(MySqlConnection connection);

    /// <summary>
    /// Gets a recipe.
    /// </summary>
    /// <param name="connection">
    /// The database connection.
    /// </param>
    /// <param name="id">
    /// The recipe ID.
    /// </param>
    /// <returns>
    /// A task for the operation.
    /// </returns>
    /// <exception cref="NotFoundException">
    /// No matching recipe was found.
    /// </exception>
    Task<Recipe> GetRecipe(MySqlConnection connection, long id);

    /// <summary>
    /// Gets the ten most recently added recipes.
    /// </summary>
    /// <param name="connection">
    /// The database connection.
    /// </param>
    /// <returns>
    /// A task for the operation.
    /// </returns>
    Task<IList<Recipe>> GetRecentlyAddedRecipes(MySqlConnection connection);

    /// <summary>
    /// Gets the ten most recently updated recipes.
    /// </summary>
    /// <remarks>
    /// Recipes that haven't been updated since they were added, and those with the IDs specified in
    /// <paramref name="excludeRecipeIds" />, are excluded from this list.
    /// </remarks>
    /// <param name="connection">
    /// The database connection.
    /// </param>
    /// <param name="excludeRecipeIds">
    /// The IDs of the recipes that should be excluded.
    /// </param>
    /// <returns>
    /// A task for the operation.
    /// </returns>
    Task<IList<Recipe>> GetRecentlyUpdatedRecipes(
        MySqlConnection connection, IReadOnlyCollection<long> excludeRecipeIds);

    /// <summary>
    /// Gets a batch of recipes.
    /// </summary>
    /// <param name="connection">
    /// The database connection.
    /// </param>
    /// <param name="ids">
    /// The recipe IDs.
    /// </param>
    /// <returns>
    /// A task for the operation. The result the list of recipes with matching IDs.
    /// </returns>
    Task<IList<Recipe>> GetRecipes(MySqlConnection connection, IReadOnlyCollection<long> ids);

    /// <summary>
    /// Updates a recipe.
    /// </summary>
    /// <param name="connection">
    /// The database connection.
    /// </param>
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
    /// A task for the operation.
    /// </returns>
    /// <exception cref="NotFoundException">
    /// No matching recipe was found.
    /// </exception>
    /// <exception cref="ConcurrencyException">
    /// <paramref name="baseRevision"/> does not match the current revision in the database.
    /// </exception>
    Task UpdateRecipe(
        MySqlConnection connection,
        long id,
        RecipeAttributes newAttributes,
        int baseRevision,
        long currentUserId);
}
