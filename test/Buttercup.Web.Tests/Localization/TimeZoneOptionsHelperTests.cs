using System;
using Microsoft.Extensions.Localization;
using Moq;
using Xunit;

namespace Buttercup.Web.Localization
{
    public class TimeZoneOptionsHelperTests
    {
        #region OptionForTimeZone

        [Fact]
        public void OptionForTimeZoneProvidesTimeZoneId()
        {
            var context = new Context();

            context.StubGetTimeZone();

            Assert.Equal(context.TimeZoneId, context.OptionForTimeZone().Id);
        }

        [Theory]
        [InlineData(2, -3)]
        [InlineData(4, -2)]
        public void OptionForTimeZoneProvidesCurrentOffset(int month, int expectedOffsetHours)
        {
            var context = new Context();

            var adjustmentRule = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(
                DateTime.MinValue,
                DateTime.MaxValue,
                new TimeSpan(1, 0, 0),
                TimeZoneInfo.TransitionTime.CreateFixedDateRule(new DateTime(0), 3, 1),
                TimeZoneInfo.TransitionTime.CreateFixedDateRule(new DateTime(0), 5, 1));

            context.StubGetTimeZone(new TimeSpan(-3, 0, 0), new[] { adjustmentRule });

            context.MockClock.SetupGet(x => x.UtcNow).Returns(
                new DateTime(2000, month, 15, 0, 0, 0, DateTimeKind.Utc));

            Assert.Equal(
                new TimeSpan(expectedOffsetHours, 0, 0), context.OptionForTimeZone().CurrentOffset);
        }

        [Theory]
        [InlineData(-1, "Format_NegativeOffset")]
        [InlineData(0, "Format_PositiveOffset")]
        [InlineData(1, "Format_PositiveOffset")]
        public void OptionForTimeZoneProvidesFormattedOffset(int offsetHours, string expectedFormat)
        {
            var context = new Context();

            var offset = new TimeSpan(offsetHours, 0, 0);

            context.StubGetTimeZone(offset);

            context.MockLocalizer
                .SetupGet(x => x[expectedFormat, offset])
                .Returns(new LocalizedString(string.Empty, "localized-offset"));

            Assert.Equal("localized-offset", context.OptionForTimeZone().FormattedOffset);
        }

        [Fact]
        public void OptionForTimeZoneProvidesCity()
        {
            var context = new Context();

            context.StubGetTimeZone();

            context.MockLocalizer
                .SetupGet(x => x[$"City_{context.TimeZoneId}"])
                .Returns(new LocalizedString(string.Empty, "localized-city-name"));

            Assert.Equal("localized-city-name", context.OptionForTimeZone().City);
        }

        #endregion

        private class Context
        {
            public Context() => this.TimeZoneOptionsHelper = new TimeZoneOptionsHelper(
                this.MockClock.Object, this.MockLocalizer.Object, this.MockTimeZoneRegistry.Object);

            public string TimeZoneId { get; } = "Sample/Time_Zone";

            public Mock<IClock> MockClock { get; } = new Mock<IClock>();

            public Mock<IStringLocalizer<TimeZoneOptionsHelper>> MockLocalizer { get; } =
                new Mock<IStringLocalizer<TimeZoneOptionsHelper>>();

            public Mock<ITimeZoneRegistry> MockTimeZoneRegistry { get; } =
                new Mock<ITimeZoneRegistry>();

            public TimeZoneOptionsHelper TimeZoneOptionsHelper { get; }

            public void StubGetTimeZone() => this.StubGetTimeZone(TimeSpan.Zero);

            public void StubGetTimeZone(TimeSpan baseUtcOffset) => this.StubGetTimeZone(
                baseUtcOffset, Array.Empty<TimeZoneInfo.AdjustmentRule>());

            public void StubGetTimeZone(
                TimeSpan baseUtcOffset, TimeZoneInfo.AdjustmentRule[] adjustmentRules)
            {
                var timeZone = TimeZoneInfo.CreateCustomTimeZone(
                    this.TimeZoneId,
                    baseUtcOffset,
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    adjustmentRules);

                this.MockTimeZoneRegistry
                    .Setup(x => x.GetTimeZone(this.TimeZoneId))
                    .Returns(timeZone);
            }

            public TimeZoneOption OptionForTimeZone() =>
                this.TimeZoneOptionsHelper.OptionForTimeZone(this.TimeZoneId);
        }
    }
}
