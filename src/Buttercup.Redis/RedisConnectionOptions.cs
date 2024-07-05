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
    /// The maximum gap allowed between errors before the grace period is reset.
    /// </summary>
    /// <remarks>
    /// This threshold is required because, for performance reasons, we don't explicitly reset the
    /// grace period after every successful Redis command.
    /// </remarks>
    public TimeSpan DroppedConnectionEpisodeTimeout { get; set; } = TimeSpan.FromSeconds(60);

    /// <summary>
    /// The minimum time to wait between forced reconnection attempts.
    /// </summary>
    public TimeSpan MinForcedReconnectionInterval { get; set; } = TimeSpan.FromSeconds(60);
}
