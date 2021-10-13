using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MySqlConnector;

namespace Buttercup.DataAccess
{
    /// <summary>
    /// The default implementation of <see cref="IMySqlConnectionSource" />.
    /// </summary>
    internal class MySqlConnectionSource : IMySqlConnectionSource
    {
        private readonly string connectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="MySqlConnectionSource" /> class.
        /// </summary>
        /// <param name="optionsAccessor">
        /// The data access options accessor.
        /// </param>
        public MySqlConnectionSource(IOptions<DataAccessOptions> optionsAccessor)
        {
            if (string.IsNullOrEmpty(optionsAccessor.Value.ConnectionString))
            {
                throw new ArgumentException(
                    "ConnectionString must not be null or empty",
                    nameof(optionsAccessor));
            }

            this.connectionString = new MySqlConnectionStringBuilder(
                optionsAccessor.Value.ConnectionString)
            {
                DateTimeKind = MySqlDateTimeKind.Utc,
            }.ToString();
        }

        /// <inheritdoc />
        public async Task<MySqlConnection> OpenConnection()
        {
            var connection = new MySqlConnection(this.connectionString);

            await connection.OpenAsync();

            return connection;
        }
    }
}
