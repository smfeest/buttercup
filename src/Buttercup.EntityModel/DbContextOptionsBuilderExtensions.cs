
using Microsoft.EntityFrameworkCore;

namespace Buttercup.EntityModel;

/// <summary>
/// Extends <see cref="DbContextOptionsBuilder" /> to facilitate configuration of database contexts.
/// </summary>
public static class DbContextOptionsBuilderExtensions
{
    /// <summary>
    /// Adds the default options for the application database.
    /// </summary>
    /// <param name="options">
    /// The options builder.
    /// </param>
    /// <param name="connectionString">
    /// The connection string.
    /// </param>
    /// <returns>
    /// The same options builder so that calls can be chained.
    /// </returns>
    public static DbContextOptionsBuilder UseAppDbOptions(
        this DbContextOptionsBuilder options,
        string connectionString) =>
        options
            .UseNpgsql(
                connectionString,
                npgOptions => npgOptions
                    .MigrationsAssembly("Buttercup.EntityModel.Migrations")
                    .MigrationsHistoryTable("__migrations_history")
                    .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .UseSnakeCaseNamingConvention();
}
