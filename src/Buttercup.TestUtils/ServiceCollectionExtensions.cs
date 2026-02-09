using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Buttercup.TestUtils;

/// <summary>
/// Provides methods for setting up service collections in tests.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds a singleton <see cref="IConfiguration"/> instance built from an in-memory collection of
    /// configuration values.
    /// </summary>
    /// <param name="services">
    /// The service collection.
    /// </param>
    /// <param name="configValues">
    /// The configuration values.
    /// </param>
    /// <returns>
    /// The service collection, for chaining.
    /// </returns>
    public static IServiceCollection AddInMemoryConfiguration(
        this IServiceCollection services,
        IEnumerable<KeyValuePair<string, string?>> configValues) =>
        services.AddSingleton<IConfiguration>(
            new ConfigurationBuilder().AddInMemoryCollection(configValues).Build());
}
