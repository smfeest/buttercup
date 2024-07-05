using StackExchange.Redis;

namespace Buttercup.Redis;

/// <summary>
/// Defines the contract for the service that manages the singleton Redis connection multiplexer.
/// </summary>
public interface IRedisConnectionManager : IAsyncDisposable
{
    /// <summary>
    /// Gets the current connection multiplexer.
    /// </summary>
    /// <remarks>
    /// Await <see cref="EnsureInitialized"/> to ensure that the initial connection has been
    /// successfully initialized before reading this property.
    /// </remarks>
    /// <value>
    /// The current connection multiplexer.
    /// </value>
    /// <exception cref="InvalidOperationException">
    /// Initial connection has yet to be successfully initialized.
    /// </exception>
    IConnectionMultiplexer CurrentConnection { get; }

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
    /// is needed to recover from the rare cases where <see cref="IConnectionMultiplexer"/> fails to
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

    /// <summary>
    /// Gets a task that is resolved once the initial connection has been initialized.
    /// </summary>
    /// <returns>
    /// A task that is resolved once the initial connection has been initialized.
    /// </returns>
    Task EnsureInitialized();
}
