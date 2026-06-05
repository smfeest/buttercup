using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Buttercup.Redis;

internal sealed partial class RedisConnectionManager : IRedisConnectionManager
{
    private readonly IRedisConnectionFactory connectionFactory;
    private readonly ILogger<RedisConnectionManager> logger;
    private readonly RedisConnectionOptions options;
    private readonly TimeProvider timeProvider;

    private readonly Task initialConnectionTask;
    private volatile bool disposed;

    private IConnectionMultiplexer? currentConnection;
    private DateTimeOffset? firstErrorTime;
    private DateTimeOffset previousErrorTime;
    private long lastReconnectTicks;
    private readonly SemaphoreSlim reconnectLock = new(initialCount: 1, maxCount: 1);
    private readonly TimeSpan reconnectLockTimeout = TimeSpan.FromSeconds(15);

    public RedisConnectionManager(
        IRedisConnectionFactory connectionFactory,
        ILogger<RedisConnectionManager> logger,
        IOptions<RedisConnectionOptions> options,
        TimeProvider timeProvider)
    {
        this.connectionFactory = connectionFactory;
        this.logger = logger;
        this.options = options.Value;
        this.timeProvider = timeProvider;

        this.initialConnectionTask = this.Connect();
    }

    public IConnectionMultiplexer CurrentConnection =>
        this.currentConnection ??
        throw new InvalidOperationException(
            "Initial connection has yet to be successfully initialized");

    public async Task<bool> CheckException(Exception exception)
    {
        if (exception is not (RedisConnectionException or SocketException))
        {
            return false;
        }

        await this.HandlePotentialDroppedConnection();
        return true;
    }

    public async ValueTask DisposeAsync()
    {
        this.disposed = true;

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

    public Task EnsureInitialized() => this.initialConnectionTask;

    private async Task Connect()
    {
        this.LogCreatingNewMultiplexer();
        var newConnection = await this.connectionFactory.NewConnection();

        var oldConnection = Interlocked.Exchange(ref this.currentConnection, newConnection);
        Interlocked.Exchange(ref this.lastReconnectTicks, this.timeProvider.GetUtcNow().UtcTicks);

        if (oldConnection != null)
        {
            try
            {
                this.LogDisposingOldMultiplexer();
                await oldConnection.DisposeAsync();
            }
            catch
            {
                // Ignore any errors from the old connection
            }
        }

        if (this.disposed)
        {
            // May have been disposed while connecting

            await this.DisposeAsync();
        }
    }

    private async Task HandlePotentialDroppedConnection()
    {
        var now = this.timeProvider.GetUtcNow();

        if (!this.MinForcedReconnectionIntervalElapsed(now))
        {
            this.LogMinForcedReconnectionIntervalNotElapsed();
            return;
        }

        if (!await this.reconnectLock.WaitAsync(this.reconnectLockTimeout))
        {
            this.LogReconnectionLockTimeout();
            return;
        }

        try
        {
            if (!this.firstErrorTime.HasValue)
            {
                this.LogFirstErrorSinceLastReconnection();
                this.firstErrorTime = now;
                this.previousErrorTime = now;
                return;
            }

            if (!this.MinForcedReconnectionIntervalElapsed(now))
            {
                this.LogMinForcedReconnectionIntervalNotElapsed();
                return;
            }

            var elapsedSinceFirstError = now - this.firstErrorTime;
            var elapsedSinceMostRecentError = now - this.previousErrorTime;

            if (elapsedSinceFirstError < this.options.DroppedConnectionGracePeriod)
            {
                this.LogDroppedConnectionGracePeriodNotElapsed();
                this.previousErrorTime = now;
                return;
            }

            if (elapsedSinceMostRecentError > this.options.DroppedConnectionEpisodeTimeout)
            {
                this.LogDroppedConnectionEpisodeTimeoutElapsed();
                this.previousErrorTime = now;
                return;
            }

            this.firstErrorTime = null;

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

    [LoggerMessage(
        EventId = 1,
        EventName = "CreatingNewMultiplexer",
        Level = LogLevel.Information,
        Message = "Creating new multiplexer")]
    private partial void LogCreatingNewMultiplexer();

    [LoggerMessage(
        EventId = 2,
        EventName = "DisposingOldMultiplexer",
        Level = LogLevel.Information,
        Message = "Disposing old multiplexer")]
    private partial void LogDisposingOldMultiplexer();

    [LoggerMessage(
        EventId = 3,
        EventName = "MinForcedReconnectionIntervalNotElapsed",
        Level = LogLevel.Debug,
        Message = "No forced reconnection; insufficient time has elapsed since last reconnection")]
    private partial void LogMinForcedReconnectionIntervalNotElapsed();

    [LoggerMessage(
        EventId = 4,
        EventName = "ReconnectionLockTimeout",
        Level = LogLevel.Debug,
        Message = "No forced reconnection; timeout waiting for reconnection lock")]
    private partial void LogReconnectionLockTimeout();

    [LoggerMessage(
        EventId = 5,
        EventName = "FirstErrorSinceLastReconnection",
        Level = LogLevel.Debug,
        Message = "No forced reconnection; first error since last reconnection")]
    private partial void LogFirstErrorSinceLastReconnection();

    [LoggerMessage(
        EventId = 6,
        EventName = "DroppedConnectionGracePeriodNotElapsed",
        Level = LogLevel.Debug,
        Message = "No forced reconnection; insufficient time has elapsed since first error")]
    private partial void LogDroppedConnectionGracePeriodNotElapsed();

    [LoggerMessage(
        EventId = 7,
        EventName = "DroppedConnectionEpisodeTimeoutElapsed",
        Level = LogLevel.Debug,
        Message = "No forced reconnection; too much time has elapsed between errors")]
    private partial void LogDroppedConnectionEpisodeTimeoutElapsed();
}
