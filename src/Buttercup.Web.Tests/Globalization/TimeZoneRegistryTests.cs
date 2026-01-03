using Xunit;

namespace Buttercup.Web.Globalization;

public sealed class TimeZoneRegistryTests
{
    #region GetSupportedTimeZones

    [Fact]
    public void GetSupportedTimeZones_ReturnsSupportedTimeZones() =>
        Assert.Equal(
            TimeZoneInfo.GetSystemTimeZones(), new TimeZoneRegistry().GetSupportedTimeZones());

    #endregion

    #region GetTimeZone

    [Fact]
    public void GetTimeZone_ReturnsRequestedTimeZone()
    {
        const string id = "Etc/GMT-8";

        var timeZone = new TimeZoneRegistry().GetTimeZone(id);

        Assert.Equal(id, timeZone.Id);
    }

    #endregion
}
