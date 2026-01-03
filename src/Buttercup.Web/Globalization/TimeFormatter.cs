using Microsoft.Extensions.Localization;

namespace Buttercup.Web.Globalization;

public sealed class TimeFormatter(IStringLocalizer<TimeFormatter> localizer) : ITimeFormatter
{
    private readonly IStringLocalizer<TimeFormatter> localizer = localizer;

    public string AsHoursAndMinutes(int totalMinutes)
    {
        if (totalMinutes < 60)
        {
            return this.FormatMinutes(totalMinutes);
        }

        var minutes = totalMinutes % 60;
        var hours = (totalMinutes - minutes) / 60;

        return minutes == 0 ?
            this.FormatHours(hours) :
            $"{this.FormatHours(hours)} {this.FormatMinutes(minutes)}";
    }

    private string FormatHours(int hours) =>
        this.localizer[hours == 1 ? "Format_Hour" : "Format_Hours", hours]!;

    private string FormatMinutes(int minutes) =>
        this.localizer[minutes == 1 ? "Format_Minute" : "Format_Minutes", minutes]!;
}
