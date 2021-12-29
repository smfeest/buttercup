namespace Buttercup.Web.Localization
{
    /// <summary>
    /// Represents a time zone option.
    /// </summary>
    public class TimeZoneOption
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimeZoneOption"/> class.
        /// </summary>
        /// <param name="id">
        /// The time zone ID.
        /// </param>
        /// <param name="currentOffset">
        /// The time zone's current UTC offset.
        /// </param>
        /// <param name="formattedOffset">
        /// A localized string representation of the time zone's current UTC offset.
        /// </param>
        /// <param name="city">
        /// The localized name of the time zone's exemplar city.
        /// </param>
        public TimeZoneOption(
            string id, TimeSpan currentOffset, string formattedOffset, string city)
        {
            this.Id = id;
            this.CurrentOffset = currentOffset;
            this.FormattedOffset = formattedOffset;
            this.City = city;
        }

        /// <summary>
        /// Gets the time zone ID.
        /// </summary>
        /// <value>
        /// The time zone ID.
        /// </value>
        public string Id { get; }

        /// <summary>
        /// Gets the time zone's current UTC offset.
        /// </summary>
        /// <value>
        /// The time zone's current UTC offset.
        /// </value>
        public TimeSpan CurrentOffset { get; }

        /// <summary>
        /// Gets a localized string representation of the time zone's current UTC offset.
        /// </summary>
        /// <value>
        /// A localized string representation of the time zone's current UTC offset.
        /// </value>
        public string FormattedOffset { get; }

        /// <summary>
        /// Gets the localized name of the time zone's exemplar city.
        /// </summary>
        /// <value>
        /// The localized name of the time zone's exemplar city.
        /// </value>
        public string City { get; }

        /// <summary>
        /// Gets a localized description of the time zone.
        /// </summary>
        /// <value>
        /// A localized description of the time zone featuring its current UTC offset and exemplar
        /// city.
        /// </value>
        public string Description => $"{this.FormattedOffset} - {this.City}";
    }
}
