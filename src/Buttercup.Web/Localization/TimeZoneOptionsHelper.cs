using System;
using Microsoft.Extensions.Localization;

namespace Buttercup.Web.Localization
{
    public class TimeZoneOptionsHelper : ITimeZoneOptionsHelper
    {
        private readonly IClock clock;
        private readonly IStringLocalizer<TimeZoneOptionsHelper> localizer;
        private readonly ITimeZoneRegistry timeZoneRegistry;

        public TimeZoneOptionsHelper(
            IClock clock,
            IStringLocalizer<TimeZoneOptionsHelper> localizer,
            ITimeZoneRegistry timeZoneRegistry)
        {
            this.clock = clock;
            this.localizer = localizer;
            this.timeZoneRegistry = timeZoneRegistry;
        }

        public TimeZoneOption OptionForTimeZone(string timeZoneId)
        {
            var timeZone = this.timeZoneRegistry.GetTimeZone(timeZoneId);

            var offset = timeZone.GetUtcOffset(this.clock.UtcNow);
            var offsetFormat = offset < TimeSpan.Zero ?
                "Format_NegativeOffset" : "Format_PositiveOffset";
            var formattedOffset = this.localizer[offsetFormat, offset];

            var city = this.localizer[$"City_{timeZone.Id}"];

            return new TimeZoneOption(timeZone.Id, offset, formattedOffset, city);
        }
    }
}
