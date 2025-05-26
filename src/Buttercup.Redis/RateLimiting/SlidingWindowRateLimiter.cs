using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Buttercup.Redis.RateLimiting;

internal sealed partial class SlidingWindowRateLimiter(
    ILogger<SlidingWindowRateLimiter> logger,
    IRedisConnectionManager redisConnectionManager,
    TimeProvider timeProvider)
    : ISlidingWindowRateLimiter
{
    private readonly ILogger<SlidingWindowRateLimiter> logger = logger;
    private readonly IRedisConnectionManager redisConnectionManager = redisConnectionManager;
    private readonly TimeProvider timeProvider = timeProvider;

    public async Task<bool> IsAllowed(string key, SlidingWindowRateLimit rateLimit)
    {
        var redisKey = RedisKey(key);

        await this.redisConnectionManager.EnsureInitialized();

        try
        {
            var currentTicks = this.timeProvider.GetUtcNow().Ticks;
            var ticksPerSegment = rateLimit.Window.Ticks / rateLimit.SegmentsPerWindow;
            var currentSegmentNumber = currentTicks / ticksPerSegment;
            var windowStartSegmentNumber = currentSegmentNumber - rateLimit.SegmentsPerWindow;

            var database = this.redisConnectionManager.CurrentConnection.GetDatabase();
            var segmentCounts = await database.HashGetAllAsync(redisKey);
            var totalCountAcrossWindow = segmentCounts
                .Where(entry => (long)entry.Name > windowStartSegmentNumber)
                .Sum(entry => (long)entry.Value);

            if (totalCountAcrossWindow >= rateLimit.Limit)
            {
                this.LogLimitExceeded(key);
                return false;
            }

            var batch = database.CreateBatch();
            var batchTasks = new Task[]
            {
                batch.HashIncrementAsync(
                    redisKey, currentSegmentNumber, 1, CommandFlags.FireAndForget),
                batch.HashFieldExpireAsync(
                    redisKey,
                    [currentSegmentNumber],
                    rateLimit.Window,
                    ExpireWhen.HasNoExpiry,
                    CommandFlags.FireAndForget)
            };
            batch.Execute();

            await Task.WhenAll(batchTasks);

            return true;
        }
        catch (Exception e)
        {
            await this.redisConnectionManager.CheckException(e);
            throw;
        }
    }

    public async Task Reset(string key)
    {
        await this.redisConnectionManager.EnsureInitialized();

        try
        {
            var database = this.redisConnectionManager.CurrentConnection.GetDatabase();
            await database.KeyDeleteAsync(RedisKey(key));
            this.LogReset(key);
        }
        catch (Exception e)
        {
            await this.redisConnectionManager.CheckException(e);
            throw;
        }
    }

    private static string RedisKey(string key) => $"rate_limit:sliding_window:{key}";

    [LoggerMessage(
        EventId = 1,
        EventName = "LimitExceeded",
        Level = LogLevel.Information,
        Message = "Rate limit exceeded for {key}")]
    private partial void LogLimitExceeded(string key);

    [LoggerMessage(
        EventId = 2,
        EventName = "CountersReset",
        Level = LogLevel.Information,
        Message = "Counters reset for {key}")]
    private partial void LogReset(string key);
}
