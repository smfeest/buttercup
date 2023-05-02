using Buttercup.EntityModel;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Xunit;

namespace Buttercup.DataAccess;

/// <summary>
/// A fixture that recreates the test database before tests in <see cref="DatabaseCollection" /> are
/// executed.
/// </summary>
public class DatabaseCollectionFixture : IAsyncLifetime
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

    private static readonly Lazy<ServerVersion> serverVersion =
        new(() => ServerVersion.AutoDetect(connectionString));

    /// <summary>
    /// Creates a new <see cref="AppDbContext" /> that connects to the test database.
    /// </summary>
    /// <returns>
    /// The new <see cref="AppDbContext" />.
    /// </returns>
    public AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder()
            .UseAppDbOptions(connectionString, serverVersion.Value)
            .Options;

        return new(options);
    }

    private async Task RecreateDatabase()
    {
        using var dbContext = CreateDbContext();

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }

    Task IAsyncLifetime.InitializeAsync() => this.RecreateDatabase();

    Task IAsyncLifetime.DisposeAsync() => Task.CompletedTask;
}
