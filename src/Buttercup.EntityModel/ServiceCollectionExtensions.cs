using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Buttercup.EntityModel;

/// <summary>
/// Extends <see cref="IServiceCollection" /> to facilitate registration of the application database
/// context factory.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the application database context factory.
    /// </summary>
    /// <param name="services">
    /// The service collection.
    /// </param>
    /// <param name="connectionString">
    /// The database connection string.
    /// </param>
    /// <returns>
    /// The service collection to allow chaining.
    /// </returns>
    public static IServiceCollection AddAppDbContextFactory(
        this IServiceCollection services, string connectionString) => services
            .AddSingleton<ServerVersion>(ServerVersion.AutoDetect(connectionString))
            .AddPooledDbContextFactory<AppDbContext>((serviceProvider, options) =>
                options.UseAppDbOptions(connectionString,
                serviceProvider.GetRequiredService<ServerVersion>()));
}
