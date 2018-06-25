using System.Data.Common;
using System.Threading.Tasks;
using Buttercup.Models;

namespace Buttercup.DataAccess
{
    /// <summary>
    /// Defines the contract for the password reset token data provider.
    /// </summary>
    public interface IPasswordResetTokenDataProvider
    {
        /// <summary>
        /// Inserts a password reset token.
        /// </summary>
        /// <param name="connection">
        /// The database connection.
        /// </param>
        /// <param name="userId">
        /// The user ID.
        /// </param>
        /// <param name="token">
        /// The token.
        /// </param>
        /// <returns>
        /// A task for the operation.
        /// </returns>
        Task InsertToken(DbConnection connection, long userId, string token);
    }
}
