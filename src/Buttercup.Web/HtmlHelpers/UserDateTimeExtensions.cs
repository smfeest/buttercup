using System.Globalization;
using Buttercup.Web.Globalization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Buttercup.Web.HtmlHelpers;

public static class UserDateTimeExtensions
{
    /// <summary>
    /// Returns a <c>time</c> element that contains a formatted date and time in the current user's
    /// time zone.
    /// </summary>
    /// <param name="helper">
    /// The HTML helper.
    /// </param>
    /// <param name="dateTime">
    /// The data and time.
    /// </param>
    /// <param name="format">
    /// The format string.
    /// </param>
    /// <returns>
    /// The <c>time</c> element.
    /// </returns>
    public static IHtmlContent UserDateTime(
        this IHtmlHelper helper, DateTime dateTime, string format = "G")
    {
        var culture = CultureInfo.CurrentCulture;
        var generalWithTzPattern =
            $"{culture.DateTimeFormat.ShortDatePattern} {culture.DateTimeFormat.LongTimePattern} zzz";
        var userDateTime = helper.ViewContext.HttpContext.ToUserTime(dateTime);

        var builder = new TagBuilder("time");
        builder.MergeAttribute("datetime", userDateTime.ToString("u", culture));
        builder.MergeAttribute("title", userDateTime.ToString(generalWithTzPattern, culture));
        builder.InnerHtml.SetContent(userDateTime.ToString(format, culture));

        return builder;
    }
}
