using Buttercup.Redis.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Buttercup.Redis;

public sealed class ServiceCollectionExtensionsTests
{
    private const string FakeConnectionString = "fake-connection-string";

    #region AddRedisServices

    [Fact]
    public void AddRedisServices_AddsRedisConnectionFactory() =>
        Assert.Contains(
            new ServiceCollection().AddRedisServices(ConfigureRedisConnectionOptions),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IRedisConnectionFactory) &&
                serviceDescriptor.ImplementationType == typeof(RedisConnectionFactory) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddRedisServices_AddsRedisConnectionManagerAsSingleton() =>
        Assert.Contains(
            new ServiceCollection().AddRedisServices(ConfigureRedisConnectionOptions),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(IRedisConnectionManager) &&
                serviceDescriptor.ImplementationType == typeof(RedisConnectionManager) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Singleton);

    [Fact]
    public void AddRedisServices_AddsSlidingWindowRateLimiter() =>
        Assert.Contains(
            new ServiceCollection().AddRedisServices(ConfigureRedisConnectionOptions),
            serviceDescriptor =>
                serviceDescriptor.ServiceType == typeof(ISlidingWindowRateLimiter) &&
                serviceDescriptor.ImplementationType == typeof(SlidingWindowRateLimiter) &&
                serviceDescriptor.Lifetime == ServiceLifetime.Transient);

    [Fact]
    public void AddRedisServices_WithConfigureActionConfiguresOptions()
    {
        var options = new ServiceCollection()
            .AddRedisServices(ConfigureRedisConnectionOptions)
            .BuildServiceProvider()
            .GetRequiredService<IOptions<RedisConnectionOptions>>();

        Assert.Equal(FakeConnectionString, options.Value.ConnectionString);
    }

    [Fact]
    public void AddRedisServices_WithConfigurationBindsConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>()
            {
                ["ConnectionString"] = FakeConnectionString,
            })
            .Build();

        var options = new ServiceCollection()
            .AddRedisServices(configuration)
            .BuildServiceProvider()
            .GetRequiredService<IOptions<RedisConnectionOptions>>();

        Assert.Equal(FakeConnectionString, options.Value.ConnectionString);
    }

    [Fact]
    public void AddRedisServices_ValidatesRedisConnectionOptions()
    {
        var options = new ServiceCollection()
            .AddRedisServices(options => { })
            .BuildServiceProvider()
            .GetRequiredService<IOptions<RedisConnectionOptions>>();

        Assert.Throws<OptionsValidationException>(() => options.Value);
    }

    private static void ConfigureRedisConnectionOptions(RedisConnectionOptions options) =>
        options.ConnectionString = FakeConnectionString;

    #endregion
}
