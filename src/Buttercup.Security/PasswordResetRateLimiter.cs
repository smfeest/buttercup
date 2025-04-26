using Buttercup.Redis.RateLimiting;
using Microsoft.Extensions.Options;

namespace Buttercup.Security;

internal sealed partial class PasswordResetRateLimiter(
    IOptions<SecurityOptions> options, ISlidingWindowRateLimiter slidingWindowRateLimiter)
    : IPasswordResetRateLimiter
{
    private readonly ISlidingWindowRateLimiter slidingWindowRateLimiter = slidingWindowRateLimiter;
    private readonly PasswordResetRateLimits rateLimits =
        options.Value.PasswordResetRateLimits;

    public async Task<bool> IsAllowed(string email) =>
        await this.slidingWindowRateLimiter.IsAllowed(
            "password_reset", this.rateLimits.Global) &&
        await this.slidingWindowRateLimiter.IsAllowed(
            $"password_reset:{email.ToLowerInvariant()}", this.rateLimits.PerEmail);
}
