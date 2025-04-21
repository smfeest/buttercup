namespace Buttercup.Security;

/// <summary>
/// Defines the contract for the password authentication rate limiter.
/// </summary>
public interface IPasswordAuthenticationRateLimiter
{
    /// <summary>
    /// Determines whether a password authentication attempt should be allowed to proceed based on
    /// the password authentication rate limit.
    /// </summary>
    /// <param name="email">The email address.</param>
    /// <returns>
    /// A task for the operation. <b>true</b> if the rate of password authentication attempts with
    /// the specified email address is within the password authentication rate limit; otherwise,
    /// <b>false</b>.
    /// </returns>
    Task<bool> IsAllowed(string email);

    /// <summary>
    /// Resets the counters for a specific email address.
    /// </summary>
    /// <param name="email">The email address.</param>
    /// <returns>A task for the operation.</returns>
    Task Reset(string email);
}
