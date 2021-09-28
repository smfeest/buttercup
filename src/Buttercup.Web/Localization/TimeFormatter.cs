using Microsoft.Extensions.Localization;

namespace Buttercup.Web.Localization
{
    public class TimeFormatter : ITimeFormatter
    {
        private readonly IStringLocalizer<TimeFormatter> localizer;

        public TimeFormatter(IStringLocalizer<TimeFormatter> localizer) =>
            this.localizer = localizer;

        public string AsHoursAndMinutes(int totalMinutes)
        {
            if (totalMinutes < 60)
            {
                return this.FormatMinutes(totalMinutes);
            }

            var minutes = totalMinutes % 60;
            var hours = (totalMinutes - minutes) / 60;

            if (minutes == 0)
            {
                return this.FormatHours(hours);
            }
            else
            {
                return $"{this.FormatHours(hours)} {this.FormatMinutes(minutes)}";
            }
        }

        private string FormatHours(int hours) =>
            this.localizer[hours == 1 ? "Format_Hour" : "Format_Hours", hours]!;

        private string FormatMinutes(int minutes) =>
            this.localizer[minutes == 1 ? "Format_Minute" : "Format_Minutes", minutes]!;
    }
}
