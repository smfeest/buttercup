using System.Data.Common;
using System.Threading.Tasks;
using MySqlConnector;

namespace Buttercup.DataAccess
{
    /// <summary>
    /// The default implementation of <see cref="IDbConnectionSource" />.
    /// </summary>
    internal class DbConnectionSource : IDbConnectionSource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DbConnectionSource" /> class.
        /// </summary>
        /// <param name="connectionString">
        /// The connection string.
        /// </param>
        public DbConnectionSource(string connectionString) =>
            this.ConnectionString = connectionString;

        /// <summary>
        /// Gets the connection string.
        /// </summary>
        /// <value>
        /// The connection string.
        /// </value>
        public string ConnectionString { get; }

        /// <inheritdoc />
        public async Task<DbConnection> OpenConnection()
        {
            var connection = new MySqlConnection(this.ConnectionString);

            await connection.OpenAsync();

            return connection;
        }
    }
}
