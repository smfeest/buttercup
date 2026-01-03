namespace Buttercup.Web.Globalization;

public sealed class TimeZoneRegistry : ITimeZoneRegistry
{
    public IList<TimeZoneInfo> GetSupportedTimeZones() => TimeZoneInfo.GetSystemTimeZones();

    public TimeZoneInfo GetTimeZone(string id) => TimeZoneInfo.FindSystemTimeZoneById(id);
}
