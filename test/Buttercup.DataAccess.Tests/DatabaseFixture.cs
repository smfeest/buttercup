using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Xunit;

namespace Buttercup.DataAccess
{
    /// <summary>
    /// A fixture that provides access to a MySQL test database.
    /// </summary>
    public class DatabaseFixture : IAsyncLifetime
    {
        public DatabaseFixture()
        {
            var configuration = new ConfigurationBuilder().AddJsonFile("db_settings.json").Build();
            this.ConnectionString = configuration.GetValue<string>("ConnectionString");
            this.DatabaseName = configuration.GetValue<string>("DatabaseName");
        }

        /// <summary>
        /// Gets the connection string for connecting to the database server.
        /// </summary>
        /// <value>
        /// The connection string for connecting to the database server.
        /// </value>
        public string ConnectionString { get; }

        /// <summary>
        /// Gets the database name.
        /// </summary>
        /// <value>
        /// The database name.
        /// </value>
        public string DatabaseName { get; }

        public Task InitializeAsync() => this.RecreateDatabase();

        public Task DisposeAsync() => Task.CompletedTask;

        private async Task RecreateDatabase()
        {
            using (var connection = new MySqlConnection(this.ConnectionString))
            {
                await connection.OpenAsync();

                using (var textReader = File.OpenText("schema.sql"))
                {
                    var scriptReader = new MySqlScriptReader(textReader);

                    string commandText;

                    while ((commandText = await scriptReader.ReadStatement()) != null)
                    {
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandText = commandText.Replace(
                                "{DatabaseName}",
                                this.DatabaseName,
                                StringComparison.OrdinalIgnoreCase);
                            await command.ExecuteNonQueryAsync();
                        }
                    }
                }
            }
        }
    }
}
