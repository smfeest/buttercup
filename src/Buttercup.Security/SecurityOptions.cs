using System.ComponentModel.DataAnnotations;
using Buttercup.Redis.RateLimiting;

namespace Buttercup.Security;

/// <summary>
/// The security options.
/// </summary>
public sealed class SecurityOptions
{
    /// <summary>
    /// The rate limit for password authentication attempts.
    /// </summary>
    [Required]
    public required SlidingWindowRateLimit PasswordAuthenticationRateLimit { get; set; }

    /// <summary>
    /// The rate limits for password reset requests.
    /// </summary>
    [Required]
    public required PasswordResetRateLimits PasswordResetRateLimits { get; set; }
}
