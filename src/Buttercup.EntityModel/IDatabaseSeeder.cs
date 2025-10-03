using Microsoft.EntityFrameworkCore;

namespace Buttercup.EntityModel;

/// <summary>
/// Defines the contract for a database seeder.
/// </summary>
public interface IDatabaseSeeder
{
    /// <summary>
    /// Seeds the database synchronously.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    void SeedDatabase(DbContext dbContext);

    /// <summary>
    /// Seeds the database synchronously.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    Task SeedDatabaseAsync(DbContext dbContext, CancellationToken cancellationToken);
}
