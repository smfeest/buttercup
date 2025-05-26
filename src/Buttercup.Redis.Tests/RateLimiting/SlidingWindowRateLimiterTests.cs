using Buttercup.Redis.TestUtils;
using Buttercup.TestUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;
using Moq;
using StackExchange.Redis;
using Xunit;

namespace Buttercup.Redis.RateLimiting;

public sealed class SlidingWindowRateLimiterTests
{
    private readonly FakeLogger<SlidingWindowRateLimiter> logger = new();
    private readonly FakeTimeProvider timeProvider = new();

    #region IsAllowed

    [Fact]
    public async Task IsAllowed_ReturnsTrueOrFalseBasedOnLimit()
    {
        var connection = await RedisConnection.GetConnection();
        var rateLimiter = new SlidingWindowRateLimiter(
            this.logger, new FakeRedisConnectionManager(connection), this.timeProvider);

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

        LogAssert.SingleEntry(this.logger)
            .HasId(1)
            .HasLevel(LogLevel.Information)
            .HasMessage($"Rate limit exceeded for {key1}");
        this.logger.Collector.Clear();

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
        var rateLimiter = new SlidingWindowRateLimiter(
            this.logger, connectionManager, this.timeProvider);

        var expectedException = new RedisException("Fake exception");
        connectionMock.Setup(x => x.GetDatabase(-1, null)).Throws(expectedException);

        Assert.Same(
            expectedException,
            await Assert.ThrowsAsync<RedisException>(
                () => rateLimiter.IsAllowed("Foo", new(1, 100))));
        Assert.Same(expectedException, Assert.Single(connectionManager.CheckedExceptions));
    }

    #endregion

    #region Reset

    [Fact]
    public async Task Reset_ResetsCountersForSpecifiedKeyOnly()
    {
        var connection = await RedisConnection.GetConnection();
        var rateLimiter = new SlidingWindowRateLimiter(
            this.logger, new FakeRedisConnectionManager(connection), this.timeProvider);

        var keySuffix = Random.Shared.Next();
        var key1 = $"Foo{keySuffix}";
        var key2 = $"Bar{keySuffix}";
        var rateLimit = new SlidingWindowRateLimit(1, 100);

        await rateLimiter.IsAllowed(key1, rateLimit);
        await rateLimiter.IsAllowed(key2, rateLimit);

        await rateLimiter.Reset(key2);

        Assert.False(await rateLimiter.IsAllowed(key1, rateLimit));
        Assert.True(await rateLimiter.IsAllowed(key2, rateLimit));

        LogAssert.SingleEntry(this.logger, 2)
            .HasLevel(LogLevel.Information)
            .HasMessage($"Counters reset for {key2}");
    }

    [Fact]
    public async Task Reset_ChecksAndRethrowsExceptions()
    {
        var connectionMock = new Mock<IConnectionMultiplexer>();
        var connectionManager = new FakeRedisConnectionManager(connectionMock.Object);
        var rateLimiter = new SlidingWindowRateLimiter(
            this.logger, connectionManager, this.timeProvider);

        var expectedException = new RedisException("Fake exception");
        connectionMock.Setup(x => x.GetDatabase(-1, null)).Throws(expectedException);

        Assert.Same(
            expectedException,
            await Assert.ThrowsAsync<RedisException>(() => rateLimiter.Reset("Foo")));
        Assert.Same(expectedException, Assert.Single(connectionManager.CheckedExceptions));
    }

    #endregion
}
