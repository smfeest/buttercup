using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace Buttercup.Redis;

public sealed class ServiceCollectionExtensionsTests
{
    private const string ConnectionString = "localhost";

    #region AddRedis

    [Fact]
    public async Task AddRedis_AddsRedisConnectionManagerFactoryAsSingleton()
    {
        var serviceProvider = new ServiceCollection()
            .AddRedis(ConfigureRedisConnectionOptions)
            .AddTransient<TimeProvider, FakeTimeProvider>()
            .BuildServiceProvider();

        var factory1 = serviceProvider.GetRequiredService<Task<IRedisConnectionManager>>();
        var factory2 = serviceProvider.GetRequiredService<Task<IRedisConnectionManager>>();

        Assert.Same(factory1, factory2);
        Assert.IsType<RedisConnectionManager>(await factory1);
    }

    [Fact]
    public void AddRedis_WithConfigureActionConfiguresOptions()
    {
        var options = new ServiceCollection()
            .AddRedis(ConfigureRedisConnectionOptions)
            .BuildServiceProvider()
            .GetRequiredService<IOptions<RedisConnectionOptions>>();

        Assert.Equal(ConnectionString, options.Value.ConnectionString);
    }

    [Fact]
    public void AddRedis_WithConfigurationBindsConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>()
            {
                ["ConnectionString"] = ConnectionString,
            })
            .Build();

        var options = new ServiceCollection()
            .AddRedis(configuration)
            .BuildServiceProvider()
            .GetRequiredService<IOptions<RedisConnectionOptions>>();

        Assert.Equal(ConnectionString, options.Value.ConnectionString);
    }

    [Fact]
    public void AddRedis_ValidatesRedisConnectionOptions()
    {
        var options = new ServiceCollection()
            .AddRedis(options => { })
            .BuildServiceProvider()
            .GetRequiredService<IOptions<RedisConnectionOptions>>();

        Assert.Throws<OptionsValidationException>(() => options.Value);
    }

    private static void ConfigureRedisConnectionOptions(RedisConnectionOptions options) =>
        options.ConnectionString = ConnectionString;

    #endregion
}
