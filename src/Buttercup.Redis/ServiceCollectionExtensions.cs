using Buttercup.Redis.RateLimiting;
using Microsoft.Extensions.DependencyInjection;

namespace Buttercup.Redis;

/// <summary>
/// Extends <see cref="IServiceCollection" /> to facilitate the addition of Redis services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Redis services to the service collection.
    /// </summary>
    /// <param name="services">
    /// The service collection.
    /// </param>
    /// <returns>
    /// The service collection to allow chaining.
    /// </returns>
    public static IServiceCollection AddRedisServices(this IServiceCollection services)
    {
        services
            .AddOptions<RedisConnectionOptions>()
            .BindConfiguration("Redis")
            .ValidateDataAnnotations();

        return services
            .AddTransient<IRedisConnectionFactory, RedisConnectionFactory>()
            .AddSingleton<IRedisConnectionManager, RedisConnectionManager>()
            .AddTransient<ISlidingWindowRateLimiter, SlidingWindowRateLimiter>();
    }
}
