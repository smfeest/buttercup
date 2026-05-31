using StackExchange.Redis;

namespace Buttercup.Redis.TestUtils;

public sealed class RedisFixture : IAsyncDisposable
{
    private readonly Lazy<Task<ConnectionMultiplexer>> lazyMultiplexerTask =
        new(() => ConnectionMultiplexer.ConnectAsync(
            "localhost,abortConnect=false,name=buttercup-tests"));

    public Task<ConnectionMultiplexer> GetMultiplexer() => this.lazyMultiplexerTask.Value;

    public async ValueTask DisposeAsync()
    {
        if (this.lazyMultiplexerTask.IsValueCreated)
        {
            var multiplexer = await this.GetMultiplexer();
            await multiplexer.DisposeAsync();
        }
    }
}
