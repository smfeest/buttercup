namespace Buttercup.Redis.RateLimiting;

/// <summary>
/// Defines the contract for a sliding window rate limiter.
/// </summary>
public interface ISlidingWindowRateLimiter
{
    /// <summary>
    /// Determines whether a request should be permitted based on a sliding window rate limit.
    /// </summary>
    /// <param name="key">The rate limit key.</param>
    /// <param name="rateLimit">The rate limit.</param>
    /// <returns>
    /// A task for the operation. <b>true</b> if the rate of requests with the specified key is
    /// within the specified rate limit; otherwise, <b>false</b>.
    /// </returns>
    Task<bool> IsAllowed(string key, SlidingWindowRateLimit rateLimit);
}
