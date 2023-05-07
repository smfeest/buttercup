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
    /// Recreates the test database.
    /// </summary>
    /// <returns>
    /// A task for the operation.
    /// </returns>
    public static async Task Recreate()
    {
        using var dbContext = CreateDbContext();

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }
}
