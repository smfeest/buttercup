using Buttercup.Redis.RateLimiting;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Buttercup.Security;

public sealed class PasswordResetRateLimiterTests
{
    private readonly PasswordResetRateLimits rateLimits = new()
    {
        Global = new(1, 100),
        PerEmail = new(2, 200),
    };
    private readonly Mock<ISlidingWindowRateLimiter> slidingWindowRateLimiterMock = new();

    private readonly PasswordResetRateLimiter passwordResetRateLimiter;

    public PasswordResetRateLimiterTests()
    {
        var options = Options.Create(
            new SecurityOptions
            {
                PasswordAuthenticationRateLimit = null!,
                PasswordResetRateLimits = this.rateLimits,
            });

        this.passwordResetRateLimiter = new(
            options, this.slidingWindowRateLimiterMock.Object);
    }

    #region IsAllowed

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, false)]
    public async Task IsAllowed_ChecksBothRateLimitsAndNormalizesEmail(
        bool withinGlobalRateLimit, bool withinPerEmailRateLimit, bool expectedResult)
    {
        this.slidingWindowRateLimiterMock
            .Setup(x => x.IsAllowed("password_reset", this.rateLimits.Global))
            .ReturnsAsync(withinGlobalRateLimit);
        this.slidingWindowRateLimiterMock
            .Setup(x => x.IsAllowed("password_reset:user@example.com", this.rateLimits.PerEmail))
            .ReturnsAsync(withinPerEmailRateLimit);

        Assert.Equal(
            expectedResult, await this.passwordResetRateLimiter.IsAllowed("User@example.COM"));
    }

    #endregion
}
