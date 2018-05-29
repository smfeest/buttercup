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
        /// Gets all the recipes ordered by title.
        /// </summary>
        /// <param name="connection">
        /// The database connection.
        /// </param>
        /// <returns>
        /// A task for the operation.
        /// </returns>
        Task<IList<Recipe>> GetRecipes(DbConnection connection);
    }
}
