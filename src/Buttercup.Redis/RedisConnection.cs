using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System.Net.Sockets;

namespace Buttercup.Redis;

internal sealed class RedisConnection : IAsyncDisposable, IRedisConnection
{
    private ConnectionMultiplexer? connection;
    private readonly Task<ConnectionMultiplexer> initialConnectionTask;
    private readonly RedisConnectionOptions options;
    private readonly TimeProvider timeProvider;

    private DateTimeOffset? firstErrorTime;
    private long lastReconnectTicks;
    private readonly SemaphoreSlim reconnectLock = new(initialCount: 1, maxCount: 1);
    private readonly TimeSpan reconnectLockTimeout = TimeSpan.FromSeconds(15);

    public RedisConnection(IOptions<RedisConnectionOptions> options, TimeProvider timeProvider)
    {
        this.options = options.Value;
        this.timeProvider = timeProvider;

        this.initialConnectionTask = this.Connect();
    }

    public Task<ConnectionMultiplexer> GetCurrentMultiplexer()
    {
        var connection = this.connection;
        return connection is null ? this.initialConnectionTask : Task.FromResult(connection);
    }

    public async Task<bool> CheckException(Exception exception)
    {
        if (exception is not RedisConnectionException or SocketException)
        {
            return false;
        }

        await this.HandlePotentialDroppedConnection();
        return true;
    }

    private async Task<ConnectionMultiplexer> Connect()
    {
        this.firstErrorTime = null;

        var newConnection = await ConnectionMultiplexer.ConnectAsync(this.options.ConnectionString);
        var oldConnection = Interlocked.Exchange(ref this.connection, newConnection);
        Interlocked.Exchange(ref this.lastReconnectTicks, this.timeProvider.GetUtcNow().UtcTicks);

        if (oldConnection != null)
        {
            try
            {
                await oldConnection.DisposeAsync();
            }
            catch
            {
                // Ignore any errors from the old connection
            }
        }

        return newConnection;
    }

    private async Task HandlePotentialDroppedConnection()
    {
        if (!this.MinForcedReconnectionIntervalElapsed() ||
            !await this.reconnectLock.WaitAsync(this.reconnectLockTimeout))
        {
            return;
        }

        try
        {
            if (!this.firstErrorTime.HasValue)
            {
                this.firstErrorTime = this.timeProvider.GetUtcNow();
                return;
            }

            if (!this.MinForcedReconnectionIntervalElapsed())
            {
                return;
            }

            var elapsedSinceFirstError = this.timeProvider.GetUtcNow() - this.firstErrorTime;

            if (elapsedSinceFirstError >= this.options.DroppedConnectionGracePeriod
                && elapsedSinceFirstError <= this.options.MaxDroppedConnectionEpisodeDuration)
            {
                await this.Connect();
            }
        }
        finally
        {
            this.reconnectLock.Release();
        }
    }

    private bool MinForcedReconnectionIntervalElapsed()
    {
        var lastReconnectTime = new DateTimeOffset(
            Interlocked.Read(ref this.lastReconnectTicks), TimeSpan.Zero);
        var timeSinceLastReconnect = this.timeProvider.GetUtcNow() - lastReconnectTime;
        return timeSinceLastReconnect < this.options.MinForcedReconnectionInterval;
    }

    public async ValueTask DisposeAsync()
    {
        if (this.connection is not null)
        {
            try
            {
                await this.connection.DisposeAsync();
            }
            catch
            {
                // Ignore any errors from disposed connection
            }
        }
    }
}
