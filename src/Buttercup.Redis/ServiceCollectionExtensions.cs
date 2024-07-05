using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Buttercup.Redis;

/// <summary>
/// Extends <see cref="IServiceCollection" /> to facilitate the addition of Redis connection
/// services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Redis connection services to the service collection.
    /// </summary>
    /// <param name="services">
    /// The service collection.
    /// </param>
    /// <param name="configure">
    /// An action that configures the Redis connection options.
    /// </param>
    /// <returns>
    /// The service collection to allow chaining.
    /// </returns>
    public static IServiceCollection AddRedisConnectionServices(
        this IServiceCollection services, Action<RedisConnectionOptions> configure) =>
        services.AddRedisConnectionServices(options => options.Configure(configure));

    /// <summary>
    /// Adds Redis connection services to the service collection.
    /// </summary>
    /// <param name="services">
    /// The service collection.
    /// </param>
    /// <param name="configuration">
    /// The configuration the Redis connection options should be bound against.
    /// </param>
    /// <returns>
    /// The service collection to allow chaining.
    /// </returns>
    public static IServiceCollection AddRedisConnectionServices(
        this IServiceCollection services, IConfiguration configuration) =>
        services.AddRedisConnectionServices(options => options.Bind(configuration));

    private static IServiceCollection AddRedisConnectionServices(
        this IServiceCollection services,
        Action<OptionsBuilder<RedisConnectionOptions>> buildOptionsAction)
    {
        buildOptionsAction(services.AddOptions<RedisConnectionOptions>().ValidateDataAnnotations());
        return services.AddSingleton<IRedisConnection, RedisConnection>();
    }
}
