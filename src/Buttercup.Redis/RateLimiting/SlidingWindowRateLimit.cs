using System.ComponentModel.DataAnnotations;

namespace Buttercup.Redis.RateLimiting;

/// <summary>
/// Defines a sliding window rate limit.
/// </summary>
public sealed record SlidingWindowRateLimit
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SlidingWindowRateLimit"/> class.
    /// </summary>
    public SlidingWindowRateLimit()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SlidingWindowRateLimit"/> class.
    /// </summary>
    /// <param name="limit">
    /// The maximum number of requests allowed within the sliding window.
    /// </param>
    /// <param name="window">
    /// The sliding window duration.
    /// </param>
    public SlidingWindowRateLimit(long limit, TimeSpan window)
    {
        this.Limit = limit;
        this.Window = window;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SlidingWindowRateLimit"/> class.
    /// </summary>
    /// <param name="limit">
    /// The maximum number of requests allowed within the sliding window.
    /// </param>
    /// <param name="windowMilliseconds">
    /// The sliding window duration in milliseconds.
    /// </param>
    public SlidingWindowRateLimit(long limit, long windowMilliseconds)
        : this(limit, TimeSpan.FromMilliseconds(windowMilliseconds))
    {
    }

    /// <summary>
    /// The maximum number of requests allowed within the sliding window.
    /// </summary>
    [Required]
    public long Limit { get; set; }

    /// <summary>
    /// The number of segments each window is divided into.
    /// </summary>
    public int SegmentsPerWindow { get; set; } = 10;

    /// <summary>
    /// The sliding window duration.
    /// </summary>
    [Required]
    public TimeSpan Window { get; set; }
}
