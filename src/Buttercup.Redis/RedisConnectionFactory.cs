using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Buttercup.Redis;

internal sealed class RedisConnectionFactory(IOptions<RedisConnectionOptions> options)
    : IRedisConnectionFactory
{
    private readonly string connectionString = options.Value.ConnectionString;

    public async Task<IConnectionMultiplexer> NewConnection() =>
        await ConnectionMultiplexer.ConnectAsync(this.connectionString);
}
