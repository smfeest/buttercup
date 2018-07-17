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
        /// Deletes all password reset tokens that are more than 24 hours old.
        /// </summary>
        /// <param name="connection">
        /// The database connection.
        /// </param>
        /// <returns>
        /// A task for the operation.
        /// </returns>
        Task DeleteExpiredTokens(DbConnection connection);

        /// <summary>
        /// Tries to get the user ID associated with a password reset token.
        /// </summary>
        /// <param name="connection">
        /// The database connection.
        /// </param>
        /// <param name="token">
        /// The password reset token.
        /// </param>
        /// <returns>
        /// A task for the operation. The result is the user ID, or a null reference if no matching
        /// token is found.
        /// </returns>
        Task<long?> GetUserIdForToken(DbConnection connection, string token);

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
