using System.Globalization;
using Buttercup.Web.Localization;
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
        var userDateTime = helper.ViewContext.HttpContext.ToUserTime(dateTime);
        var uFormatted = userDateTime.ToString("u", CultureInfo.CurrentCulture);

        var builder = new TagBuilder("time");
        builder.MergeAttribute("datetime", uFormatted);
        builder.MergeAttribute("title", uFormatted);
        builder.InnerHtml.SetContent(userDateTime.ToString(format, CultureInfo.CurrentCulture));

        return builder;
    }
}
