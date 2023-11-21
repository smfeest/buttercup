using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace Buttercup;

public sealed class TimeProviderExtensionsTests
{
    #region GetUtcDateTimeNow

    [Fact]
    public void GetUtcDateTimeNow_ReturnsCurrentUtcTime()
    {
        var timeNow = new DateTimeOffset(2000, 1, 2, 3, 4, 5, 6, TimeSpan.FromHours(1));
        var timeProvider = new FakeTimeProvider(timeNow);

        var result = timeProvider.GetUtcDateTimeNow();

        Assert.Equal(timeNow.UtcDateTime, result);
        Assert.Equal(DateTimeKind.Utc, result.Kind);
    }

    #endregion
}
