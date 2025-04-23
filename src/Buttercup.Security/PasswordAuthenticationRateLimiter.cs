using Buttercup.Redis.RateLimiting;
using Microsoft.Extensions.Options;

namespace Buttercup.Security;

internal sealed partial class PasswordAuthenticationRateLimiter(
    IOptions<SecurityOptions> options, ISlidingWindowRateLimiter slidingWindowRateLimiter)
    : IPasswordAuthenticationRateLimiter
{
    private readonly ISlidingWindowRateLimiter slidingWindowRateLimiter = slidingWindowRateLimiter;
    private readonly SlidingWindowRateLimit rateLimit =
        options.Value.PasswordAuthenticationRateLimit;

    public Task<bool> IsAllowed(string email) =>
        this.slidingWindowRateLimiter.IsAllowed(RateLimitKey(email), this.rateLimit);

    public Task Reset(string email) => this.slidingWindowRateLimiter.Reset(RateLimitKey(email));

    private static string RateLimitKey(string email) =>
        $"password_authentication:{email.ToLowerInvariant()}";
}
