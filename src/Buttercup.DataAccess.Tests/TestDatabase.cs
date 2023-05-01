using Buttercup.EntityModel;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

namespace Buttercup.DataAccess;

/// <summary>
/// Provides methods for creating and connecting to the test database.
/// </summary>
public static class TestDatabase
{
    private const string Server = "localhost";
    private const string User = "buttercup_dev";
    private const string DatabaseName = "buttercup_test";

    private static readonly Lazy<ServerVersion> serverVersion = new(
        () => ServerVersion.AutoDetect(BuildConnectionString()));

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
    /// Creates a new <see cref="AppDbContext" /> that connects to the test database.
    /// </summary>
    /// <returns>
    /// The new <see cref="AppDbContext" />.
    /// </returns>
    public static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder()
            .UseAppDbOptions(BuildConnectionString(), serverVersion.Value)
            .Options;

        return new(options);
    }

    /// <summary>
    /// Opens a connection to the test database with an open transaction
    /// that will be automatically rolled back when the connection is
    /// disposed.
    /// </summary>
    /// <returns>
    /// A task for the operation. The result is the new connection.
    /// </returns>
    public static async Task<MySqlConnection> OpenConnectionWithRollback()
    {
        var connection = new MySqlConnection(BuildConnectionString(
            builder => builder.IgnoreCommandTransaction = true));

        await connection.OpenAsync();

        await connection.BeginTransactionAsync();

        return connection;
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

    private static async Task ExecuteCommand(MySqlConnection connection, string commandText)
    {
        using var command = connection.CreateCommand();

        command.CommandText = commandText;

        await command.ExecuteNonQueryAsync();
    }
}
