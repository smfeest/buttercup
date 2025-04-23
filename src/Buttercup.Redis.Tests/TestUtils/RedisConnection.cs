using StackExchange.Redis;

namespace Buttercup.Redis.TestUtils;

public static class RedisConnection
{
    private static readonly Lazy<Task<ConnectionMultiplexer>> lazyConnectionTask =
        new(() => ConnectionMultiplexer.ConnectAsync(
            "localhost,abortConnect=false,name=buttercup-tests"));

    public static Task<ConnectionMultiplexer> GetConnection() => lazyConnectionTask.Value;
}
