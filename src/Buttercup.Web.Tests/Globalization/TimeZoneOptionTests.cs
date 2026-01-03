using Xunit;

namespace Buttercup.Web.Globalization;

public sealed class TimeZoneOptionTests
{
    #region Description

    [Fact]
    public void Description_ConcatenatesFormattedOffsetAndCity()
    {
        var timeZoneOption = new TimeZoneOption(
            string.Empty, TimeSpan.Zero, "sample-offset", "sample-city");

        Assert.Equal("sample-offset - sample-city", timeZoneOption.Description);
    }

    #endregion
}
