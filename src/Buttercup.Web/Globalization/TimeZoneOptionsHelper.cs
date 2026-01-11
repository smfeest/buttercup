using System.Globalization;
using Microsoft.Extensions.Localization;

namespace Buttercup.Web.Globalization;

public sealed partial class TimeZoneOptionsHelper(
    IStringLocalizer<TimeZoneOptionsHelper> localizer,
    ILogger<TimeZoneOptionsHelper> logger,
    TimeProvider timeProvider,
    ITimeZoneRegistry timeZoneRegistry)
    : ITimeZoneOptionsHelper
{
    private readonly IStringLocalizer<TimeZoneOptionsHelper> localizer = localizer;
    private readonly ILogger<TimeZoneOptionsHelper> logger = logger;
    private readonly TimeProvider clock = timeProvider;
    private readonly ITimeZoneRegistry timeZoneRegistry = timeZoneRegistry;

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
        var offset = timeZone.GetUtcOffset(this.clock.GetUtcDateTimeNow());
        var offsetFormat = offset < TimeSpan.Zero ?
            "Format_NegativeOffset" : "Format_PositiveOffset";
        var formattedOffset = this.localizer[offsetFormat, offset];

        var cityTranslation = this.localizer[$"City_{timeZone.Id}"];

        string city;

        if (cityTranslation.ResourceNotFound)
        {
            this.LogMissingCityTranslation(CultureInfo.CurrentUICulture.Name, timeZone.Id);
            city = timeZone.Id;
        }
        else
        {
            city = cityTranslation.Value;
        }

        return new(timeZone.Id, offset, formattedOffset, city);
    }

    [LoggerMessage(
        EventId = 1,
        EventName = "MissingCityTranslation",
        Level = LogLevel.Debug,
        Message = "Missing {UiCulture} city translation for {TimeZoneId}")]
    private partial void LogMissingCityTranslation(string uiCulture, string timeZoneId);
}
