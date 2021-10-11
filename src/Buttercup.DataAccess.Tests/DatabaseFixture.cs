using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using MySqlConnector;
using Xunit;

namespace Buttercup.DataAccess
{
    /// <summary>
    /// A fixture that provides access to a MySQL test database.
    /// </summary>
    public class DatabaseFixture : IAsyncLifetime
    {
        private const string Server = "localhost";
        private const string User = "buttercup_dev";
        private const string DatabaseName = "buttercup_test";

        public Task InitializeAsync() => this.RecreateDatabase();

        public Task DisposeAsync() => Task.CompletedTask;

        /// <summary>
        /// Builds a connection string to connect to the test database.
        /// </summary>
        /// <param name="configure">
        /// A callback that can be used to customize the connection string
        /// builder.
        /// </param>
        /// <returns>
        /// The connection string.
        /// </returns>
        public string BuildConnectionString(Action<MySqlConnectionStringBuilder>? configure = null)
        {
            var builder = new MySqlConnectionStringBuilder
            {
                Server = Server,
                UserID = User,
                Database = DatabaseName,
            };

            configure?.Invoke(builder);

            return builder.ToString();
        }

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
            using var connection = new MySqlConnection(
                this.BuildConnectionString(builder => builder.IgnoreCommandTransaction = true));

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

        private async Task RecreateDatabase()
        {
            using var connection = new MySqlConnection(
                this.BuildConnectionString(builder => builder.Database = null));

            await connection.OpenAsync();

            await ExecuteCommand(
                connection,
                $"DROP DATABASE IF EXISTS `{DatabaseName}`;CREATE DATABASE `{DatabaseName}`");

            await connection.ChangeDatabaseAsync(DatabaseName);

            var commandText = await File.ReadAllTextAsync("schema.sql");

            await ExecuteCommand(connection, commandText);
        }
    }
}
