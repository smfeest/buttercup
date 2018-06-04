using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Buttercup.Models;

namespace Buttercup.DataAccess
{
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
        /// <param name="recipe">
        /// The recipe.
        /// </param>
        /// <returns>
        /// A task for the operation. The task result is the ID of the new recipe.
        /// </returns>
        Task<long> AddRecipe(DbConnection connection, Recipe recipe);

        /// <summary>
        /// Deletes a recipe.
        /// </summary>
        /// <param name="connection">
        /// The database connection.
        /// </param>
        /// <param name="id">
        /// The recipe ID.
        /// </param>
        /// <param name="revision">
        /// The current revision. Used to prevent lost updates.
        /// </param>
        /// <returns>
        /// A task for the operation.
        /// </returns>
        /// <exception cref="NotFoundException">
        /// No matching recipe was found.
        /// </exception>
        /// <exception cref="ConcurrencyException">
        /// <paramref name="revision"/> does not match the revision in the database.
        /// </exception>
        Task DeleteRecipe(DbConnection connection, long id, int revision);

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
        Task<Recipe> GetRecipe(DbConnection connection, long id);

        /// <summary>
        /// Gets all the recipes ordered by title.
        /// </summary>
        /// <param name="connection">
        /// The database connection.
        /// </param>
        /// <returns>
        /// A task for the operation.
        /// </returns>
        Task<IList<Recipe>> GetRecipes(DbConnection connection);

        /// <summary>
        /// Gets the ten most recently added recipes.
        /// </summary>
        /// <param name="connection">
        /// The database connection.
        /// </param>
        /// <returns>
        /// A task for the operation.
        /// </returns>
        Task<IList<Recipe>> GetRecentlyAddedRecipes(DbConnection connection);

        /// <summary>
        /// Gets the ten most recently updated recipes.
        /// </summary>
        /// <remarks>
        /// Recipes that haven't been updated since they were added, and those that are within the
        /// ten most recently added, are excluded from this list.
        /// </remarks>
        /// <param name="connection">
        /// The database connection.
        /// </param>
        /// <returns>
        /// A task for the operation.
        /// </returns>
        Task<IList<Recipe>> GetRecentlyUpdatedRecipes(DbConnection connection);

        /// <summary>
        /// Updates a recipe.
        /// </summary>
        /// <param name="connection">
        /// The database connection.
        /// </param>
        /// <param name="recipe">
        /// The recipe.
        /// </param>
        /// <returns>
        /// A task for the operation.
        /// </returns>
        /// <exception cref="NotFoundException">
        /// No matching recipe was found.
        /// </exception>
        /// <exception cref="ConcurrencyException">
        /// The revision in <paramref name="recipe"/> does not match the revision in the database.
        /// </exception>
        Task UpdateRecipe(DbConnection connection, Recipe recipe);
    }
}
