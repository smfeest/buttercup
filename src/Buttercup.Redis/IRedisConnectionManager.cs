using StackExchange.Redis;

namespace Buttercup.Redis;

/// <summary>
/// Defines the contract for the service that manages the singleton Redis connection multiplexer.
/// </summary>
public interface IRedisConnectionManager
{
    /// <summary>
    /// Gets the current connection multiplexer.
    /// </summary>
    /// <value>
    /// The current connection multiplexer.
    /// </value>
    ConnectionMultiplexer CurrentConnection { get; }

    /// <summary>
    /// Checks whether an exception indicates a possible dropped connection and attempts to replace
    /// the current connection multiplexer subject to <see
    /// cref="RedisConnectionOptions.DroppedConnectionGracePeriod"/>, <see
    /// cref="RedisConnectionOptions.DroppedConnectionEpisodeTimeout"/> and <see
    /// cref="RedisConnectionOptions.MinForcedReconnectionInterval"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The method is intended to be called whenever an exception is thrown by a call to Redis. It
    /// is needed to recover from the rare cases where <see cref="ConnectionMultiplexer"/> fails to
    /// automatically reconnect after a dropped connection.
    /// </para>
    /// </remarks>
    /// <param name="exception">
    /// The exception.
    /// </param>
    /// <returns>
    /// A task for the operation. The value is <c>true</c> if <paramref name="exception"/> indicates
    /// a possible dropped connection (so the caller may wish to retry the operation), <c>false</c>
    /// otherwise.
    /// </returns>
    Task<bool> CheckException(Exception exception);
}
