using System;
using System.Collections.Generic;

namespace Buttercup.Web.Localization
{
    /// <summary>
    /// Defines the contract for the time zone option helper.
    /// </summary>
    public interface ITimeZoneOptionsHelper
    {
        /// <summary>
        /// Creates an ordered list of all supported time zone options.
        /// </summary>
        /// <remarks>
        /// Options are ordered by offset then exemplar city name.
        /// </remarks>
        /// <returns>
        /// The list of time zone options.
        /// </returns>
        IList<TimeZoneOption> AllOptions();

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
