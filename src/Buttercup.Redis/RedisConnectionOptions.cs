using System.ComponentModel.DataAnnotations;

namespace Buttercup.Redis;

/// <summary>
/// The Redis connection options.
/// </summary>
public sealed class RedisConnectionOptions
{
    /// <summary>
    /// The connection string.
    /// </summary>
    [Required]
    public required string ConnectionString { get; set; }

    /// <summary>
    /// The minimum time that has to pass between the first and a subsequent error for a forced
    /// reconnection to be attempted.
    /// </summary>
    public TimeSpan DroppedConnectionGracePeriod { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// The minimum time that has to pass between the first and a subsequent error for a new grace
    /// period to be triggered.
    /// </summary>
    public TimeSpan MaxDroppedConnectionEpisodeDuration { get; set; } = TimeSpan.FromMinutes(60);

    /// <summary>
    /// The minimum time to wait between forced reconnection attempts.
    /// </summary>
    public TimeSpan MinForcedReconnectionInterval { get; set; } = TimeSpan.FromSeconds(60);
}
