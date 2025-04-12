using StackExchange.Redis;

namespace Buttercup.Redis.RateLimiting;

internal sealed class SlidingWindowRateLimiter(
    IRedisConnectionManager redisConnectionManager, TimeProvider timeProvider)
    : ISlidingWindowRateLimiter
{
    private readonly IRedisConnectionManager redisConnectionManager = redisConnectionManager;
    private readonly TimeProvider timeProvider = timeProvider;

    public async Task<bool> IsAllowed(string key, SlidingWindowRateLimit rateLimit)
    {
        var redisKey = $"rate_limit:sliding_window:{key}";

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
}
