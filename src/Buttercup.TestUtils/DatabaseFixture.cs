using System.Security.Cryptography;
using System.Text;
using Buttercup.EntityModel;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Xunit;

namespace Buttercup.TestUtils;

/// <summary>
/// A fixture that can be used to test components using a real database.
/// </summary>
/// <remarks>
/// Type parameter <typeparamref name="TCollection" /> is used to generate a name for the test
/// database. Collections and classes that use this fixture with a distinct type for <typeparamref
/// name="TCollection" /> can safely run in parallel.
/// </remarks>
/// <typeparam name="TCollection">
/// The collection or test class.
/// </typeparam>
public class DatabaseFixture<TCollection> : IAsyncLifetime
{
    private const string Server = "localhost";
    private const string User = "buttercup_dev";

    private readonly Lazy<ServerVersion> serverVersion;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseFixture{TCollection}" /> class.
    /// </summary>
    public DatabaseFixture()
    {
        this.DatabaseName = $"buttercup_test_{ComputeDatabaseNameSuffix()}";
        this.ConnectionString = new MySqlConnectionStringBuilder
        {
            Server = Server,
            UserID = User,
            Database = this.DatabaseName,
        }.ToString();

        this.serverVersion = new(() => ServerVersion.AutoDetect(this.ConnectionString));
    }

    /// <summary>
    /// Gets the connection string for the test database.
    /// </summary>
    public string ConnectionString { get; }

    /// <summary>
    /// Gets the name of the test database.
    /// </summary>
    public string DatabaseName { get; }

    /// <summary>
    /// Creates a new <see cref="AppDbContext" /> that connects to the test database.
    /// </summary>
    /// <returns>
    /// The new <see cref="AppDbContext" />.
    /// </returns>
    public AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder()
            .UseAppDbOptions(this.ConnectionString, serverVersion.Value)
            .Options;

        return new(options);
    }

    private async Task DeleteDatabase()
    {
        using var dbContext = CreateDbContext();

        await dbContext.Database.EnsureDeletedAsync();
    }

    private async Task RecreateDatabase()
    {
        using var dbContext = CreateDbContext();

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }

    private static string ComputeDatabaseNameSuffix()
    {
        var hash = SHA256.HashData(
            Encoding.UTF8.GetBytes(typeof(TCollection).AssemblyQualifiedName!));

        return Convert.ToHexString(hash)[..10];
    }

    Task IAsyncLifetime.InitializeAsync() => this.RecreateDatabase();

    Task IAsyncLifetime.DisposeAsync() => this.DeleteDatabase();
}
