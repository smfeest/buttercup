using Microsoft.AspNetCore.Mvc.Rendering;

namespace Buttercup.Web.Globalization;

/// <summary>
/// Provides extension methods for collections of time zone options.
/// </summary>
public static class TimeZoneOptionCollectionExtensions
{
    /// <summary>
    /// Converts a collection of time zone options to select list items.
    /// </summary>
    /// <param name="timeZoneOptions">
    /// The collection of time zone options.
    /// </param>
    /// <returns>
    /// The collection of select list items.
    /// </returns>
    public static IEnumerable<SelectListItem> AsSelectListItems(
        this IEnumerable<TimeZoneOption> timeZoneOptions) =>
            timeZoneOptions.Select(tzo => new SelectListItem(tzo.Description, tzo.Id));
}
