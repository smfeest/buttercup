using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Localization;
using Moq;
using Xunit;

namespace Buttercup.Web.Localization
{
    public class TimeZoneOptionsHelperTests
    {
        #region AllOptions

        [Fact]
        public void AllOptionsReturnsOrderedListOfOptions()
        {
            var fixture = new AllOptionsFixture();

            fixture
                .AddFakeTimeZone("tz1/+3B", 3, "city-b")
                .AddFakeTimeZone("tz2/-2", -2)
                .AddFakeTimeZone("tz3/+0", 0)
                .AddFakeTimeZone("tz4/+5", 5)
                .AddFakeTimeZone("tz5/+3A", 3, "city-a")
                .AddFakeTimeZone("tz6/-1", -1)
                .AddFakeTimeZone("tz7/+3C", 3, "city-c")
                .AddFakeTimeZone("tz8/-6", -6)
                .AddFakeTimeZone("tz9/+1", 1);

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

            var actualIds = fixture.TimeZoneOptionsHelper.AllOptions().Select(o => o.Id);

            Assert.Equal(expectedIds, actualIds);
        }

        #endregion

        #region OptionForTimeZone

        [Fact]
        public void OptionForTimeZoneProvidesTimeZoneId()
        {
            var fixture = new OptionForTimeZoneFixture();

            fixture.StubGetTimeZone();

            Assert.Equal(fixture.TimeZoneId, fixture.OptionForTimeZone().Id);
        }

        [Theory]
        [InlineData(2, -3)]
        [InlineData(4, -2)]
        public void OptionForTimeZoneProvidesCurrentOffset(int month, int expectedOffsetHours)
        {
            var fixture = new OptionForTimeZoneFixture();

            var adjustmentRule = TimeZoneInfo.AdjustmentRule.CreateAdjustmentRule(
                DateTime.MinValue,
                DateTime.MaxValue,
                new(1, 0, 0),
                TimeZoneInfo.TransitionTime.CreateFixedDateRule(new(0), 3, 1),
                TimeZoneInfo.TransitionTime.CreateFixedDateRule(new(0), 5, 1));

            fixture.StubGetTimeZone(new(-3, 0, 0), new[] { adjustmentRule });

            fixture.MockClock.SetupGet(x => x.UtcNow).Returns(
                new DateTime(2000, month, 15, 0, 0, 0, DateTimeKind.Utc));

            Assert.Equal(new(expectedOffsetHours, 0, 0), fixture.OptionForTimeZone().CurrentOffset);
        }

        [Theory]
        [InlineData(-1, "Format_NegativeOffset")]
        [InlineData(0, "Format_PositiveOffset")]
        [InlineData(1, "Format_PositiveOffset")]
        public void OptionForTimeZoneProvidesFormattedOffset(int offsetHours, string expectedFormat)
        {
            var fixture = new OptionForTimeZoneFixture();

            var offset = new TimeSpan(offsetHours, 0, 0);

            fixture.StubGetTimeZone(offset);

            fixture.MockLocalizer
                .SetupGet(x => x[expectedFormat, offset])
                .Returns(new LocalizedString(string.Empty, "localized-offset"));

            Assert.Equal("localized-offset", fixture.OptionForTimeZone().FormattedOffset);
        }

        [Fact]
        public void OptionForTimeZoneProvidesCity()
        {
            var fixture = new OptionForTimeZoneFixture();

            fixture.StubGetTimeZone();

            fixture.MockLocalizer
                .SetupGet(x => x[$"City_{fixture.TimeZoneId}"])
                .Returns(new LocalizedString(string.Empty, "localized-city-name"));

            Assert.Equal("localized-city-name", fixture.OptionForTimeZone().City);
        }

        #endregion

        private class TimeZoneOptionsHelperFixture
        {
            public TimeZoneOptionsHelperFixture() => this.TimeZoneOptionsHelper = new(
                this.MockClock.Object, this.MockLocalizer.Object, this.MockTimeZoneRegistry.Object);

            public Mock<IClock> MockClock { get; } = new();

            public Mock<IStringLocalizer<TimeZoneOptionsHelper>> MockLocalizer { get; } = new();

            public Mock<ITimeZoneRegistry> MockTimeZoneRegistry { get; } = new();

            public TimeZoneOptionsHelper TimeZoneOptionsHelper { get; }
        }

        private class AllOptionsFixture : TimeZoneOptionsHelperFixture
        {
            private readonly List<TimeZoneInfo> timeZones = new();

            public AllOptionsFixture()
            {
                this.MockTimeZoneRegistry
                    .Setup(x => x.GetSupportedTimeZones())
                    .Returns(this.timeZones);
            }

            public AllOptionsFixture AddFakeTimeZone(string timeZoneId, int offsetHours) =>
                this.AddFakeTimeZone(timeZoneId, offsetHours, string.Empty);

            public AllOptionsFixture AddFakeTimeZone(
                string timeZoneId, int offsetHours, string city)
            {
                this.timeZones.Add(TimeZoneInfo.CreateCustomTimeZone(
                    timeZoneId, new(offsetHours, 0, 0), string.Empty, string.Empty));

                this.MockLocalizer
                    .SetupGet(x => x[$"City_{timeZoneId}"])
                    .Returns(new LocalizedString(string.Empty, city));

                return this;
            }
        }

        private class OptionForTimeZoneFixture : TimeZoneOptionsHelperFixture
        {
            public string TimeZoneId { get; } = "Sample/Time_Zone";

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
