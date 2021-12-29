using Xunit;

namespace Buttercup.Web.Localization;

public class TimeZoneRegistryTests
{
    #region GetSupportedTimeZones

    [Fact]
    public void GetSupportedTimeZonesReturnsSupportedTimeZones() =>
        Assert.Equal(
            TimeZoneInfo.GetSystemTimeZones(), new TimeZoneRegistry().GetSupportedTimeZones());

    #endregion

    #region GetTimeZone

    [Fact]
    public void GetTimeZoneReturnsRequestedTimeZone()
    {
        const string id = "Etc/GMT-8";

        var timeZone = new TimeZoneRegistry().GetTimeZone(id);

        Assert.Equal(id, timeZone.Id);
    }

    #endregion
}
