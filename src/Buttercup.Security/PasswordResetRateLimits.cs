using System.ComponentModel.DataAnnotations;
using Buttercup.Redis.RateLimiting;

namespace Buttercup.Security;

/// <summary>
/// Defines the rate limits for password reset requests.
/// </summary>
public sealed record PasswordResetRateLimits
{
    /// <summary>
    /// The global rate limit for password reset requests.
    /// </summary>
    [Required]
    public required SlidingWindowRateLimit Global { get; set; }

    /// <summary>
    /// The per-email rate limit for password reset requests.
    /// </summary>
    [Required]
    public required SlidingWindowRateLimit PerEmail { get; set; }
}
