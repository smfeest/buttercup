using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using MySqlConnector;

namespace Buttercup.DataAccess
{
    /// <summary>
    /// Provides methods for creating and connecting to the test database.
    /// </summary>
    public static class TestDatabase
    {
        private const string Server = "localhost";
        private const string User = "buttercup_dev";
        private const string DatabaseName = "buttercup_test";

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
        public static string BuildConnectionString(
            Action<MySqlConnectionStringBuilder>? configure = null)
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
        /// Recreates the test database.
        /// </summary>
        /// <returns>
        /// A task for the operation.
        /// </returns>
        public static async Task Recreate()
        {
            using var connection = new MySqlConnection(
                BuildConnectionString(builder => builder.Database = null));

            await connection.OpenAsync();

            await ExecuteCommand(
                connection,
                $"DROP DATABASE IF EXISTS `{DatabaseName}`;CREATE DATABASE `{DatabaseName}`");

            await connection.ChangeDatabaseAsync(DatabaseName);

            var commandText = await File.ReadAllTextAsync("schema.sql");

            await ExecuteCommand(connection, commandText);
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
        public static async Task WithRollback(Func<MySqlConnection, Task> action)
        {
            using var connection = new MySqlConnection(
                BuildConnectionString(builder => builder.IgnoreCommandTransaction = true));

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
    }
}
