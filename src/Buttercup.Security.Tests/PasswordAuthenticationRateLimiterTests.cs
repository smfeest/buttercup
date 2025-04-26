using Buttercup.Redis.RateLimiting;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Buttercup.Security;

public sealed class PasswordAuthenticationRateLimiterTests
{
    private readonly SlidingWindowRateLimit rateLimit = new(1, 100);
    private readonly Mock<ISlidingWindowRateLimiter> slidingWindowRateLimiterMock = new();

    private readonly PasswordAuthenticationRateLimiter passwordAuthenticationRateLimiter;

    public PasswordAuthenticationRateLimiterTests()
    {
        var options = Options.Create(
            new SecurityOptions
            {
                PasswordAuthenticationRateLimit = this.rateLimit,
                PasswordResetRateLimits = null!
            });

        this.passwordAuthenticationRateLimiter = new(
            options, this.slidingWindowRateLimiterMock.Object);
    }

    #region IsAllowed

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task IsAllowed_DelegatesToSlidingWindowRateLimiterUsingNormalizedEmailInKey(
        bool expectedResult)
    {
        this.slidingWindowRateLimiterMock
            .Setup(x => x.IsAllowed("password_authentication:user@example.com", this.rateLimit))
            .ReturnsAsync(expectedResult);

        var actualResult =
            await this.passwordAuthenticationRateLimiter.IsAllowed("User@example.COM");

        Assert.Equal(expectedResult, actualResult);
    }

    #endregion

    #region Reset

    [Fact]
    public async Task Reset_DelegatesToSlidingWindowRateLimiterUsingNormalizedEmailInKey()
    {
        await this.passwordAuthenticationRateLimiter.Reset("User@example.COM");

        this.slidingWindowRateLimiterMock
            .Verify(x => x.Reset("password_authentication:user@example.com"));
    }

    #endregion
}
