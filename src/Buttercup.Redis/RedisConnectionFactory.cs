using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Buttercup.Redis;

internal sealed class RedisConnectionFactory(ILoggerFactory loggerFactory, IOptions<RedisConnectionOptions> options)
    : IRedisConnectionFactory
{
    private readonly ILoggerFactory loggerFactory = loggerFactory;
    private readonly string connectionString = options.Value.ConnectionString;

    public async Task<IConnectionMultiplexer> NewConnection() =>
        await ConnectionMultiplexer.ConnectAsync(
            this.connectionString, options => options.LoggerFactory = this.loggerFactory);
}
