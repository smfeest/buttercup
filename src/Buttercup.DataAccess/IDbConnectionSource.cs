using System.Data.Common;
using System.Threading.Tasks;

namespace Buttercup.DataAccess
{
    /// <summary>
    /// Defines the contract for the database connection source.
    /// </summary>
    public interface IDbConnectionSource
    {
        /// <summary>
        /// Opens a database connection.
        /// </summary>
        /// <returns>
        /// The database connection.
        /// </returns>
        Task<DbConnection> OpenConnection();
    }
}
