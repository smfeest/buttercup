using StackExchange.Redis;

namespace Buttercup.Redis;

internal interface IRedisConnectionFactory
{
    Task<IConnectionMultiplexer> NewConnection();
}
