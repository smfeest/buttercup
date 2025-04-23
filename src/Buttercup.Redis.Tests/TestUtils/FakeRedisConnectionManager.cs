using StackExchange.Redis;

namespace Buttercup.Redis.TestUtils;

public sealed class FakeRedisConnectionManager(IConnectionMultiplexer connection)
    : IRedisConnectionManager
{
    public List<Exception> CheckedExceptions { get; } = [];

    public IConnectionMultiplexer CurrentConnection { get; } = connection;

    public Task<bool> CheckException(Exception exception)
    {
        this.CheckedExceptions.Add(exception);
        return Task.FromResult(false);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public Task EnsureInitialized() => Task.CompletedTask;
}
