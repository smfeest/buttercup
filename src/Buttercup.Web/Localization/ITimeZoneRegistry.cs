namespace Buttercup.Web.Localization;

/// <summary>
/// Defines the contract for the time zone registry.
/// </summary>
public interface ITimeZoneRegistry
{
    /// <summary>
    /// Gets a list of all supported time zone.
    /// </summary>
    /// <returns>
    /// The supported time zones.
    /// </returns>
    IList<TimeZoneInfo> GetSupportedTimeZones();

    /// <summary>
    /// Gets a time zone.
    /// </summary>
    /// <param name="id">
    /// The TZ ID of the time zone.
    /// </param>
    /// <returns>
    /// The time zone.
    /// </returns>
    TimeZoneInfo GetTimeZone(string id);
}
