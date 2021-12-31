namespace Buttercup.Web.Localization;

/// <summary>
/// Defines the contract for the time formatter.
/// </summary>
public interface ITimeFormatter
{
    /// <summary>
    /// Formats a total number of minutes as hours and minutes in words.
    /// </summary>
    /// <param name="totalMinutes">
    /// The total number of minutes.
    /// </param>
    /// <returns>
    /// The formatted time period.
    /// </returns>
    string AsHoursAndMinutes(int totalMinutes);
}
