using System;
using System.Data.Common;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MySqlConnector;

namespace Buttercup.DataAccess
{
    /// <summary>
    /// The default implementation of <see cref="IDbConnectionSource" />.
    /// </summary>
    internal class DbConnectionSource : IDbConnectionSource
    {
        private readonly string connectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="DbConnectionSource" /> class.
        /// </summary>
        /// <param name="optionsAccessor">
        /// The data access options accessor.
        /// </param>
        public DbConnectionSource(IOptions<DataAccessOptions> optionsAccessor)
        {
            if (string.IsNullOrEmpty(optionsAccessor.Value.ConnectionString))
            {
                throw new ArgumentException(
                    "ConnectionString must not be null or empty",
                    nameof(optionsAccessor));
            }

            this.connectionString = optionsAccessor.Value.ConnectionString;
        }

        /// <inheritdoc />
        public async Task<DbConnection> OpenConnection()
        {
            var connection = new MySqlConnection(this.connectionString);

            await connection.OpenAsync();

            return connection;
        }
    }
}
