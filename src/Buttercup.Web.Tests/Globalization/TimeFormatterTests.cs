using System.Globalization;
using Microsoft.Extensions.Localization;
using Moq;
using Xunit;

namespace Buttercup.Web.Globalization;

public sealed class TimeFormatterTests
{
    #region AsHoursAndMinutes

    [Theory]
    [InlineData(0, "0 minutes")]
    [InlineData(1, "1 minute")]
    [InlineData(59, "59 minutes")]
    [InlineData(60, "1 hour")]
    [InlineData(120, "2 hours")]
    [InlineData(121, "2 hours 1 minute")]
    [InlineData(125, "2 hours 5 minutes")]
    [InlineData(1500, "25 hours")]
    public void AsHoursAndMinutes_ReturnsHoursAndMinutesInWords(
        int minutes, string expectedOutput)
    {
        var resources = new Dictionary<string, string>
        {
            ["Format_Hour"] = "{0:d} hour",
            ["Format_Hours"] = "{0:d} hours",
            ["Format_Minute"] = "{0:d} minute",
            ["Format_Minutes"] = "{0:d} minutes",
        };

        var mockLocalizer = new Mock<IStringLocalizer<TimeFormatter>>();
        mockLocalizer
            .Setup(x => x[It.IsAny<string>(), It.IsAny<object[]>()])
            .Returns((string key, object[] args) =>
                new(key, string.Format(CultureInfo.InvariantCulture, resources[key], args)));

        var timeFormatter = new TimeFormatter(mockLocalizer.Object);

        Assert.Equal(expectedOutput, timeFormatter.AsHoursAndMinutes(minutes));
    }

    #endregion
}
