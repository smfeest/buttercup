using Buttercup.Redis.RateLimiting;
using Buttercup.TestUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Buttercup.Redis;

public sealed class ServiceCollectionExtensionsTests
{
    private static readonly KeyValuePair<string, string?>[] ConfigValues =
        [new("Redis:ConnectionString", "fake-connection-string")];

    #region AddRedisServices

    [Fact]
    public void AddRedisServices_AddsRedisConnectionFactory() =>
        Assert.Contains(
            new ServiceCollection().AddInMemoryConfiguration(ConfigValues).AddRedisServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IRedisConnectionFactory) &&
                serviceDescriptor.ImplementationType == typeof(RedisConnectionFactory) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddRedisServices_AddsRedisConnectionManagerAsSingleton() =>
        Assert.Contains(
            new ServiceCollection().AddInMemoryConfiguration(ConfigValues).AddRedisServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IRedisConnectionManager) &&
                serviceDescriptor.ImplementationType == typeof(RedisConnectionManager) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Singleton);

    [Fact]
    public void AddRedisServices_AddsSlidingWindowRateLimiter() =>
        Assert.Contains(
            new ServiceCollection().AddInMemoryConfiguration(ConfigValues).AddRedisServices(),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(ISlidingWindowRateLimiter) &&
                serviceDescriptor.ImplementationType == typeof(SlidingWindowRateLimiter) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddRedisServices_BindsRedisConnectionOptions()
    {
        var options = new ServiceCollection()
            .AddInMemoryConfiguration(ConfigValues)
            .AddRedisServices()
            .BuildServiceProvider()
            .GetRequiredService<IOptions<RedisConnectionOptions>>();

        Assert.Equal(new() { ConnectionString = "fake-connection-string" }, options.Value);
    }

    [Fact]
    public void AddRedisServices_ValidatesRedisConnectionOptions()
    {
        var options = new ServiceCollection()
            .AddInMemoryConfiguration([new("Redis:ConnectionString", string.Empty)])
            .AddRedisServices()
            .BuildServiceProvider()
            .GetRequiredService<IOptions<RedisConnectionOptions>>();

        Assert.Throws<OptionsValidationException>(() => options.Value);
    }

    #endregion
}
