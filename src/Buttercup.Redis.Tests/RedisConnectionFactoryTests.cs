using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Xunit;

namespace Buttercup.Redis;

public sealed class RedisConnectionFactoryTests
{
    #region NewConnection

    [Fact]
    public async Task NewConnection_ReturnsNewConnection()
    {
        var options = new RedisConnectionOptions
        {
            ConnectionString = "localhost,abortConnect=false",
        };

        var connectionFactory = new RedisConnectionFactory(Options.Create(options));

        var connection = await connectionFactory.NewConnection();

        Assert.Equal(
            ConfigurationOptions.Parse(options.ConnectionString).ToString(),
            connection.Configuration);
    }

    #endregion
}
