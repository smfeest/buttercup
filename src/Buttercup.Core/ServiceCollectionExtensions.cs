using Microsoft.Extensions.DependencyInjection;

namespace Buttercup;

/// <summary>
/// Extends <see cref="IServiceCollection" /> to facilitate the addition of core services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds core services to the service collection.
    /// </summary>
    /// <param name="services">
    /// The service collection.
    /// </param>
    /// <returns>
    /// The service collection to allow chaining.
    /// </returns>
    public static IServiceCollection AddCoreServices(this IServiceCollection services) =>
        services
            .AddTransient<IRandomNumberGeneratorFactory, RandomNumberGeneratorFactory>()
            .AddTransient<IRandomTokenGenerator, RandomTokenGenerator>();
}
