using Xunit;

namespace Buttercup;

public sealed class ClockTests
{
    #region UtcNow

    [Fact]
    public void UtcNowReturnsCurrentUtcTime()
    {
        var start = DateTime.UtcNow;
        var actual = new Clock().UtcNow;
        var finish = DateTime.UtcNow;

        Assert.InRange(actual, start, finish);
    }

    #endregion
}
