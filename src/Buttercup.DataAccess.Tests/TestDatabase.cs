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

    private static readonly string connectionString = new MySqlConnectionStringBuilder
    {
        Server = Server,
        UserID = User,
        Database = DatabaseName,
    }.ToString();

    private static readonly Lazy<ServerVersion> serverVersion = new(
        () => ServerVersion.AutoDetect(connectionString));

    /// <summary>
    /// Creates a new <see cref="AppDbContext" /> that connects to the test database.
    /// </summary>
    /// <returns>
    /// The new <see cref="AppDbContext" />.
    /// </returns>
    public static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder()
            .UseAppDbOptions(connectionString, serverVersion.Value)
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
