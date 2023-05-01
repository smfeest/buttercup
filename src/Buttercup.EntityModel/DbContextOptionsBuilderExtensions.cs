
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

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
    /// <param name="serverVersion">
    /// The database server version.
    /// </param>
    /// <returns>
    /// The same options builder so that calls can be chained.
    /// </returns>
    public static DbContextOptionsBuilder UseAppDbOptions(
        this DbContextOptionsBuilder options,
        string connectionString,
        ServerVersion serverVersion) =>
        options
            .UseMySql(
                AddConnectionStringOptions(connectionString),
                serverVersion,
                mysqlOptions => mysqlOptions
                    .MigrationsAssembly("Buttercup.EntityModel.Migrations")
                    .MigrationsHistoryTable("__migrations_history"))
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
            .UseSnakeCaseNamingConvention();

    private static string AddConnectionStringOptions(string baseConnectionString) =>
        new MySqlConnectionStringBuilder(baseConnectionString)
        {
            DateTimeKind = MySqlDateTimeKind.Utc
        }.ToString();
}
