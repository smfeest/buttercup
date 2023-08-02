using System.Security.Claims;
using Buttercup.Security;

namespace Buttercup.Web.Localization;

/// <summary>
/// Provides extension methods for <see cref="HttpContext" />.
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// Converts a UTC date and time to a date and time in the current user's time zone.
    /// </summary>
    /// <remarks>
    /// If there is no current user the date and time is left in UTC.
    /// </remarks>
    /// <param name="httpContext">
    /// The HTTP context for the request.
    /// </param>
    /// <param name="dateTime">
    /// The UTC date and time to be converted.
    /// </param>
    /// <returns>
    /// The date and time in the current user's time zone.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="dateTime" /> is not a UTC date and time.
    /// </exception>
    public static DateTimeOffset ToUserTime(this HttpContext httpContext, DateTime dateTime)
    {
        if (dateTime.Kind != DateTimeKind.Utc)
        {
            throw new ArgumentException(
                $"{nameof(dateTime)} is not a UTC date and time", nameof(dateTime));
        }

        var utc = new DateTimeOffset(dateTime);

        var timeZone = httpContext.User.FindFirstValue(CustomClaimTypes.TimeZone);

        return timeZone == null ? utc : TimeZoneInfo.ConvertTimeBySystemTimeZoneId(utc, timeZone);
    }
}
