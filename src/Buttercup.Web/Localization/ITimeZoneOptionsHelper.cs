using System;

namespace Buttercup.Web.Localization
{
    /// <summary>
    /// Defines the contract for the time zone option helper.
    /// </summary>
    public interface ITimeZoneOptionsHelper
    {
        /// <summary>
        /// Creates a time zone option representing a time zone.
        /// </summary>
        /// <param name="timeZoneId">
        /// The time zone ID.
        /// </param>
        /// <returns>
        /// The time zone option.
        /// </returns>
        TimeZoneOption OptionForTimeZone(string timeZoneId);
    }
}
