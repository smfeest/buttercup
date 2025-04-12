using Buttercup.Redis.TestUtils;
using Microsoft.Extensions.Time.Testing;
using Moq;
using StackExchange.Redis;
using Xunit;

namespace Buttercup.Redis.RateLimiting;

public sealed class SlidingWindowRateLimiterTests
{
    private readonly FakeTimeProvider timeProvider = new();

    #region IsAllowed

    [Fact]
    public async Task IsAllowed_ReturnsTrueOrFalseBasedOnLimit()
    {
        var connection = await RedisConnection.GetConnection();
        var connectionManager = new FakeRedisConnectionManager(connection);
        var rateLimiter = new SlidingWindowRateLimiter(connectionManager, this.timeProvider);

        var keySuffix = Random.Shared.Next();
        var key1 = $"Foo{keySuffix}";
        var key2 = $"Bar{keySuffix}";

        var rateLimit = new SlidingWindowRateLimit()
        {
            Limit = 6,
            SegmentsPerWindow = 5,
            Window = TimeSpan.FromMilliseconds(50),
        };

        for (var i = 0; i < 2; i++)
        {
            Assert.True(await rateLimiter.IsAllowed(key1, rateLimit));
        }

        this.timeProvider.Advance(TimeSpan.FromMilliseconds(10));

        for (var i = 0; i < 2; i++)
        {
            Assert.True(await rateLimiter.IsAllowed(key1, rateLimit));
        }

        this.timeProvider.Advance(TimeSpan.FromMilliseconds(20));

        for (var i = 0; i < 2; i++)
        {
            Assert.True(await rateLimiter.IsAllowed(key1, rateLimit));
        }

        Assert.False(await rateLimiter.IsAllowed(key1, rateLimit));
        Assert.True(await rateLimiter.IsAllowed(key2, rateLimit));

        this.timeProvider.Advance(TimeSpan.FromMilliseconds(20));

        for (var i = 0; i < 2; i++)
        {
            Assert.True(await rateLimiter.IsAllowed(key1, rateLimit));
        }

        Assert.False(await rateLimiter.IsAllowed(key1, rateLimit));
        Assert.True(await rateLimiter.IsAllowed(key2, rateLimit));
    }

    [Fact]
    public async Task IsAllowed_ChecksAndRethrowsExceptions()
    {
        var connectionMock = new Mock<IConnectionMultiplexer>();
        var connectionManager = new FakeRedisConnectionManager(connectionMock.Object);
        var rateLimiter = new SlidingWindowRateLimiter(connectionManager, this.timeProvider);

        var expectedException = new RedisException("Fake exception");
        connectionMock.Setup(x => x.GetDatabase(-1, null)).Throws(expectedException);

        Assert.Same(
            expectedException,
            await Assert.ThrowsAsync<RedisException>(
                () => rateLimiter.IsAllowed("Foo", new(1, 100))));
        Assert.Same(expectedException, Assert.Single(connectionManager.CheckedExceptions));
    }

    #endregion
}
