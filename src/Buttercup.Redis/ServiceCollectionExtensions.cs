using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Buttercup.Redis;

/// <summary>
/// Extends <see cref="IServiceCollection" /> to facilitate the addition of the Redis connection
/// service.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Redis connection manager and options to the service collection.
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
    public static IServiceCollection AddRedis(
        this IServiceCollection services, Action<RedisConnectionOptions> configure) =>
        services.AddRedis(options => options.Configure(configure));

    /// <summary>
    /// Adds the Redis connection manager and options to the service collection.
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
    public static IServiceCollection AddRedis(
        this IServiceCollection services, IConfiguration configuration) =>
        services.AddRedis(options => options.Bind(configuration));

    private static IServiceCollection AddRedis(
        this IServiceCollection services,
        Action<OptionsBuilder<RedisConnectionOptions>> buildOptionsAction)
    {
        buildOptionsAction(services.AddOptions<RedisConnectionOptions>().ValidateDataAnnotations());

        return services.AddSingleton<Task<IRedisConnectionManager>>(
            async serviceProvider => await RedisConnectionManager.Initialize(
                serviceProvider.GetRequiredService<IOptions<RedisConnectionOptions>>().Value,
                serviceProvider.GetRequiredService<TimeProvider>()));
    }
}
