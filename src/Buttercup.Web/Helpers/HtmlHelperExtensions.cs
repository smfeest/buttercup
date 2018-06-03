using System;
using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Buttercup.Web.Helpers
{
    /// <summary>
    /// Provides extension methods for <see cref="IHtmlHelper" />.
    /// </summary>
    public static class HtmlHelperExtensions
    {
        /// <summary>
        /// Formats a time period specified in minutes as hours and minutes in words.
        /// </summary>
        /// <param name="htmlHelper">
        /// The HTML helper.
        /// </param>
        /// <param name="totalMinutes">
        /// The total number of minutes.
        /// </param>
        /// <returns>
        /// The formatted time period.
        /// </returns>
        public static string FormatAsHoursAndMinutes(this IHtmlHelper htmlHelper, int totalMinutes)
        {
            if (totalMinutes < 60)
            {
                return htmlHelper.FormatMinutes(totalMinutes);
            }

            var minutes = totalMinutes % 60;
            var hours = (totalMinutes - minutes) / 60;

            if (minutes == 0)
            {
                return htmlHelper.FormatHours(hours);
            }
            else
            {
                return $"{htmlHelper.FormatHours(hours)} {htmlHelper.FormatMinutes(minutes)}";
            }
        }

        private static string FormatHours(this IHtmlHelper htmlHelper, int hours) =>
            htmlHelper.FormatValue(hours, hours == 1 ? "{0:d} hour" : "{0:d} hours");

        private static string FormatMinutes(this IHtmlHelper htmlHelper, int minutes) =>
            htmlHelper.FormatValue(minutes, minutes == 1 ? "{0:d} minute" : "{0:d} minutes");
    }
}
