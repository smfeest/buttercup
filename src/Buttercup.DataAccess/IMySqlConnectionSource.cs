using System.Threading.Tasks;
using MySqlConnector;

namespace Buttercup.DataAccess
{
    /// <summary>
    /// Defines the contract for the database connection source.
    /// </summary>
    public interface IMySqlConnectionSource
    {
        /// <summary>
        /// Opens a database connection.
        /// </summary>
        /// <returns>
        /// The database connection.
        /// </returns>
        Task<MySqlConnection> OpenConnection();
    }
}
