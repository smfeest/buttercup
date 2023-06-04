namespace Buttercup.Web.Localization;

/// <summary>
/// Represents a time zone option.
/// </summary>
/// <param name="Id">
/// The time zone ID.
/// </param>
/// <param name="CurrentOffset">
/// The time zone's current UTC offset.
/// </param>
/// <param name="FormattedOffset">
/// A localized string representation of the time zone's current UTC offset.
/// </param>
/// <param name="City">
/// The localized name of the time zone's exemplar city.
/// </param>
public sealed record TimeZoneOption(
    string Id, TimeSpan CurrentOffset, string FormattedOffset, string City)
{
    /// <summary>
    /// Gets a localized description of the time zone.
    /// </summary>
    /// <value>
    /// A localized description of the time zone featuring its current UTC offset and exemplar
    /// city.
    /// </value>
    public string Description => $"{this.FormattedOffset} - {this.City}";
}
