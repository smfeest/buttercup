using System.Data.Common;
using System.Threading.Tasks;
using Buttercup.Models;

namespace Buttercup.DataAccess
{
    /// <summary>
    /// Defines the contract for the user data provider.
    /// </summary>
    public interface IUserDataProvider
    {
        /// <summary>
        /// Tries to find a user by email address.
        /// </summary>
        /// <param name="connection">
        /// The database connection.
        /// </param>
        /// <param name="email">
        /// The email address.
        /// </param>
        /// <returns>
        /// A task for the operation. The result is the user, or a null reference if no matching
        /// user is found.
        /// </returns>
        Task<User> FindUserByEmail(DbConnection connection, string email);
    }
}
