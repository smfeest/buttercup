using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
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
            this.DatabaseConnectionString = this.BuildDatabaseConnectionString();
        }

        /// <summary>
        /// Gets the connection string for connecting to the database server.
        /// </summary>
        /// <value>
        /// The connection string for connecting to the database server.
        /// </value>
        public string ConnectionString { get; }

        /// <summary>
        /// Gets the connection string for connecting to the test database created by this fixture.
        /// </summary>
        /// <value>
        /// The connection string for connecting to the test database created by this fixture.
        /// </value>
        public string DatabaseConnectionString { get; }

        /// <summary>
        /// Gets the database name.
        /// </summary>
        /// <value>
        /// The database name.
        /// </value>
        public string DatabaseName { get; }

        public Task InitializeAsync() => this.RecreateDatabase();

        public Task DisposeAsync() => Task.CompletedTask;

        /// <summary>
        /// Runs asynchronous code within a transaction that is rolled back on completion.
        /// </summary>
        /// <param name="action">
        /// The asynchronous action.
        /// </param>
        /// <returns>
        /// A task for the operation.
        /// </returns>
        public async Task WithRollback(Func<MySqlConnection, Task> action)
        {
            using var connection = new MySqlConnection(this.DatabaseConnectionString);

            await connection.OpenAsync();

            using var transaction = await connection.BeginTransactionAsync();

            await action(connection);
        }

        [SuppressMessage(
            "Microsoft.Security",
            "CA2100:ReviewSqlQueriesForSecurityVulnerabilities",
            Justification = "Command text does not contain user input")]
        private static async Task ExecuteCommand(MySqlConnection connection, string commandText)
        {
            using var command = connection.CreateCommand();

            command.CommandText = commandText;

            await command.ExecuteNonQueryAsync();
        }

        private string BuildDatabaseConnectionString() =>
            new MySqlConnectionStringBuilder(this.ConnectionString)
            {
                Database = this.DatabaseName,
            }.ToString();

        private async Task RecreateDatabase()
        {
            using var connection = new MySqlConnection(this.ConnectionString);

            await connection.OpenAsync();

            await ExecuteCommand(
                connection,
                $"DROP DATABASE IF EXISTS `{this.DatabaseName}`;CREATE DATABASE `{this.DatabaseName}`");

            await connection.ChangeDatabaseAsync(this.DatabaseName);

            using var textReader = File.OpenText("schema.sql");

            var scriptReader = new MySqlScriptReader(textReader);

            string? commandText;

            while ((commandText = await scriptReader.ReadStatement()) != null)
            {
                await ExecuteCommand(connection, commandText);
            }
        }
    }
}
