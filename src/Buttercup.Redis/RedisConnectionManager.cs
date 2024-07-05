using System.Net.Sockets;
using StackExchange.Redis;

namespace Buttercup.Redis;

internal sealed class RedisConnectionManager : IAsyncDisposable, IRedisConnectionManager
{
    private ConnectionMultiplexer? currentConnection;
    private readonly RedisConnectionOptions options;
    private readonly TimeProvider timeProvider;

    private DateTimeOffset? firstErrorTime;
    private DateTimeOffset previousErrorTime;
    private long lastReconnectTicks;
    private readonly SemaphoreSlim reconnectLock = new(initialCount: 1, maxCount: 1);
    private readonly TimeSpan reconnectLockTimeout = TimeSpan.FromSeconds(15);

    private RedisConnectionManager(RedisConnectionOptions options, TimeProvider timeProvider)
    {
        this.options = options;
        this.timeProvider = timeProvider;
    }

    public ConnectionMultiplexer CurrentConnection => this.currentConnection!;

    public async Task<bool> CheckException(Exception exception)
    {
        if (exception is not RedisConnectionException or SocketException)
        {
            return false;
        }

        await this.HandlePotentialDroppedConnection();
        return true;
    }

    private async Task Connect()
    {
        this.firstErrorTime = null;

        var newConnection = await ConnectionMultiplexer.ConnectAsync(this.options.ConnectionString);
        var oldConnection = Interlocked.Exchange(ref this.currentConnection, newConnection);
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
    }

    private async Task HandlePotentialDroppedConnection()
    {
        var now = this.timeProvider.GetUtcNow();

        if (!this.MinForcedReconnectionIntervalElapsed(now) ||
            !await this.reconnectLock.WaitAsync(this.reconnectLockTimeout))
        {
            return;
        }

        try
        {
            if (!this.firstErrorTime.HasValue)
            {
                this.firstErrorTime = now;
                this.previousErrorTime = now;
                return;
            }

            if (!this.MinForcedReconnectionIntervalElapsed(now))
            {
                return;
            }

            var elapsedSinceFirstError = now - this.firstErrorTime;
            var elapsedSinceMostRecentError = now - this.previousErrorTime;

            if (elapsedSinceFirstError < this.options.DroppedConnectionGracePeriod
                || elapsedSinceMostRecentError > this.options.DroppedConnectionEpisodeTimeout)
            {
                this.previousErrorTime = now;
                return;
            }

            await this.Connect();
        }
        finally
        {
            this.reconnectLock.Release();
        }
    }

    private bool MinForcedReconnectionIntervalElapsed(DateTimeOffset now)
    {
        var lastReconnectTime = new DateTimeOffset(
            Interlocked.Read(ref this.lastReconnectTicks), TimeSpan.Zero);
        var timeSinceLastReconnect = now - lastReconnectTime;
        return timeSinceLastReconnect > this.options.MinForcedReconnectionInterval;
    }

    public async ValueTask DisposeAsync()
    {
        if (this.currentConnection is not null)
        {
            try
            {
                await this.currentConnection.DisposeAsync();
            }
            catch
            {
                // Ignore any errors from disposed connection
            }
        }
    }

    public static async Task<RedisConnectionManager> Initialize(
        RedisConnectionOptions options, TimeProvider timeProvider)
    {
        var connectionManager = new RedisConnectionManager(options, timeProvider);
        await connectionManager.Connect();
        return connectionManager;
    }
}
