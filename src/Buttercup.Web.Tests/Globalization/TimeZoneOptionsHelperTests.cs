using System.Globalization;
using Buttercup.TestUtils;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Xunit;

namespace Buttercup.Web.Globalization;

public sealed class TimeZoneOptionsHelperTests
{
    private readonly DictionaryLocalizer<TimeZoneOptionsHelper> localizer = new();
    private readonly FakeLogger<TimeZoneOptionsHelper> logger = new();
    private readonly FakeTimeProvider timeProvider = new();
    private readonly Mock<ITimeZoneRegistry> timeZoneRegistryMock = new();

    private readonly TimeZoneOptionsHelper timeZoneOptionsHelper;

    public TimeZoneOptionsHelperTests() =>
        this.timeZoneOptionsHelper = new(
            this.localizer, this.logger, this.timeProvider, this.timeZoneRegistryMock.Object);

    #region AllOptions

    [Fact]
    public void AllOptions_ReturnsOrderedListOfOptions()
    {
        var timeZones = new List<TimeZoneInfo>();

        void AddFakeTimeZone(string timeZoneId, int offsetHours, string city = "")
        {
            timeZones.Add(
                TimeZoneInfo.CreateCustomTimeZone(
                    timeZoneId, new(offsetHours, 0, 0), string.Empty, string.Empty));

            this.localizer.Add($"City_{timeZoneId}", city);
        }

        AddFakeTimeZone("tz1/+3B", 3, "city-b");
        AddFakeTimeZone("tz2/-2", -2);
        AddFakeTimeZone("tz3/+0", 0);
        AddFakeTimeZone("tz4/+5", 5);
        AddFakeTimeZone("tz5/+3A", 3, "city-a");
        AddFakeTimeZone("tz6/-1", -1);
        AddFakeTimeZone("tz7/+3C", 3, "city-c");
        AddFakeTimeZone("tz8/-6", -6);
        AddFakeTimeZone("tz9/+1", 1);

        this.timeZoneRegistryMock.Setup(x => x.GetSupportedTimeZones()).Returns(timeZones);

        var expectedIds = new string[]
        {
            "tz8/-6",
            "tz2/-2",
            "tz6/-1",
            "tz3/+0",
            "tz9/+1",
            "tz5/+3A",
            "tz1/+3B",
            "tz7/+3C",
            "tz4/+5",
        };

        var actualIds = this.timeZoneOptionsHelper.AllOptions().Select(o => o.Id);

        Assert.Equal(expectedIds, actualIds);
    }

    #endregion

    #region OptionForTimeZone

    [Fact]
    public void OptionForTimeZone_ProvidesTimeZoneId()
    {
        this.StubGetTimeZone();

        var option = this.timeZoneOptionsHelper.OptionForTimeZone(FakeTimeZoneId);

        Assert.Equal(FakeTimeZoneId, option.Id);
    }

    [Theory]
    [InlineData(2, -3)]
    [InlineData(4, -2)]
    public void OptionForTimeZone_ProvidesCurrentOffset(int month, int expectedOffsetHours)
    {
        var adjustmentRule = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(
            DateTime.MinValue,
            DateTime.MaxValue,
            new(1, 0, 0),
            TimeZoneInfo.TransitionTime.CreateFixedDateRule(new(0), 3, 1),
            TimeZoneInfo.TransitionTime.CreateFixedDateRule(new(0), 5, 1));
        this.StubGetTimeZone(new(-3, 0, 0), [adjustmentRule]);
        this.timeProvider.SetUtcNow(new DateTime(2000, month, 15, 0, 0, 0));

        var option = this.timeZoneOptionsHelper.OptionForTimeZone(FakeTimeZoneId);

        Assert.Equal(new(expectedOffsetHours, 0, 0), option.CurrentOffset);
    }

    [Theory]
    [InlineData(-1, "Format_NegativeOffset")]
    [InlineData(0, "Format_PositiveOffset")]
    [InlineData(1, "Format_PositiveOffset")]
    public void OptionForTimeZone_ProvidesFormattedOffset(int offsetHours, string expectedFormat)
    {
        var offset = new TimeSpan(offsetHours, 0, 0);
        this.StubGetTimeZone(offset);
        this.localizer.Add(expectedFormat, "localized-offset / {0:c}");

        var option = this.timeZoneOptionsHelper.OptionForTimeZone(FakeTimeZoneId);

        Assert.Equal($"localized-offset / {offset:c}", option.FormattedOffset);
    }

    [Fact]
    public void OptionForTimeZone_CityTranslationAvailable_ProvidesTranslatedCityName()
    {
        this.StubGetTimeZone();
        this.localizer.Add($"City_{FakeTimeZoneId}", "localized-city-name");

        var option = this.timeZoneOptionsHelper.OptionForTimeZone(FakeTimeZoneId);

        Assert.Equal("localized-city-name", option.City);
    }

    [Fact]
    public void OptionForTimeZone_CityTranslationUnavailable_LogsDebugMessage()
    {
        this.StubGetTimeZone();

        this.timeZoneOptionsHelper.OptionForTimeZone(FakeTimeZoneId);

        LogAssert.SingleEntry(this.logger)
            .HasId(1)
            .HasLevel(LogLevel.Debug)
            .HasMessage(
                $"Missing {CultureInfo.CurrentUICulture.Name} city translation for {FakeTimeZoneId}");
    }

    [Fact]
    public void OptionForTimeZone_CityTranslationUnavailable_ProvidesTimeZoneIdAsFallback()
    {
        this.StubGetTimeZone();

        var option = this.timeZoneOptionsHelper.OptionForTimeZone(FakeTimeZoneId);

        Assert.Equal(FakeTimeZoneId, option.City);
    }

    private const string FakeTimeZoneId = "Fake/Time_Zone";

    private void StubGetTimeZone() => this.StubGetTimeZone(TimeSpan.Zero);

    private void StubGetTimeZone(TimeSpan baseUtcOffset) => this.StubGetTimeZone(baseUtcOffset, []);

    private void StubGetTimeZone(
        TimeSpan baseUtcOffset, TimeZoneInfo.AdjustmentRule[] adjustmentRules)
    {
        var timeZone = TimeZoneInfo.CreateCustomTimeZone(
            FakeTimeZoneId,
            baseUtcOffset,
            string.Empty,
            string.Empty,
            string.Empty,
            adjustmentRules);

        this.timeZoneRegistryMock
            .Setup(x => x.GetTimeZone(FakeTimeZoneId))
            .Returns(timeZone);
    }

    #endregion
}
