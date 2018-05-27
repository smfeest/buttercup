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
    }
}
