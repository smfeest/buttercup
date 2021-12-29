using Xunit;

namespace Buttercup.Web.Localization;

public class TimeZoneOptionTests
{
    #region Constructor

    [Fact]
    public void ConstructorSetsProperties()
    {
        var timeZoneOption = new TimeZoneOption(
            "sample-id", new(1, 2, 3), "sample-offset", "sample-city");

        Assert.Equal("sample-id", timeZoneOption.Id);
        Assert.Equal(new(1, 2, 3), timeZoneOption.CurrentOffset);
        Assert.Equal("sample-offset", timeZoneOption.FormattedOffset);
        Assert.Equal("sample-city", timeZoneOption.City);
    }

    #endregion

    #region Description

    [Fact]
    public void DescriptionConcatenatesFormattedOffsetAndCity()
    {
        var timeZoneOption = new TimeZoneOption(
            string.Empty, TimeSpan.Zero, "sample-offset", "sample-city");

        Assert.Equal("sample-offset - sample-city", timeZoneOption.Description);
    }

    #endregion
}
