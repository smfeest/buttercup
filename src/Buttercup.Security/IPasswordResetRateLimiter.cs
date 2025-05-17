namespace Buttercup.Security;

/// <summary>
/// Defines the contract for the password reset rate limiter.
/// </summary>
public interface IPasswordResetRateLimiter
{
    /// <summary>
    /// Determines whether a password reset request should be accepted based on the global and
    /// per-email password reset rate limits.
    /// </summary>
    /// <param name="email">The email address.</param>
    /// <returns>
    /// A task for the operation. <b>true</b> if the request is within the rate limits; otherwise,
    /// <b>false</b>.
    /// </returns>
    Task<bool> IsAllowed(string email);
}
