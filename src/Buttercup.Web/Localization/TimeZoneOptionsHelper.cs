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

        public IList<TimeZoneOption> AllOptions()
        {
            var timeZones = this.timeZoneRegistry.GetSupportedTimeZones();

            var options = timeZones.Select(this.OptionForTimeZone).ToList();

            options.Sort(CompareOptions);

            return options;
        }

        public TimeZoneOption OptionForTimeZone(string timeZoneId) =>
            this.OptionForTimeZone(this.timeZoneRegistry.GetTimeZone(timeZoneId));

        private static int CompareOptions(TimeZoneOption x, TimeZoneOption y)
        {
            var result = x.CurrentOffset.CompareTo(y.CurrentOffset);

            return result != 0 ? result : StringComparer.CurrentCulture.Compare(x.City, y.City);
        }

        private TimeZoneOption OptionForTimeZone(TimeZoneInfo timeZone)
        {
            var offset = timeZone.GetUtcOffset(this.clock.UtcNow);
            var offsetFormat = offset < TimeSpan.Zero ?
                "Format_NegativeOffset" : "Format_PositiveOffset";
            var formattedOffset = this.localizer[offsetFormat, offset];

            var city = this.localizer[$"City_{timeZone.Id}"];

            return new(timeZone.Id, offset, formattedOffset!, city!);
        }
    }
}
