using Xunit;

namespace Buttercup.Web.Localization;

public sealed class TimeZoneOptionCollectionExtensionsTests
{
    #region AsSelectListItems

    [Fact]
    public void AsSelectListItems_ConvertsTimeZoneOptionsToSelectListItems()
    {
        var timeZoneOption = new TimeZoneOption(
            "Sample/Time_Zone", TimeSpan.Zero, "Sample-Offset", "Sample-City");

        var selectListItem = new[] { timeZoneOption }.AsSelectListItems().First();

        Assert.Equal(selectListItem.Value, timeZoneOption.Id);
        Assert.Equal(selectListItem.Text, timeZoneOption.Description);
    }

    #endregion
}
