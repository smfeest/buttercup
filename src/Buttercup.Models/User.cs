namespace Buttercup.Models
{
    /// <summary>
    /// Represents a user.
    /// </summary>
    public class User
    {
        /// <summary>
        /// Gets or sets the user ID.
        /// </summary>
        /// <value>
        /// The user ID.
        /// </value>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the email address.
        /// </summary>
        /// <value>
        /// The email address.
        /// </value>
        public string? Email { get; set; }

        /// <summary>
        /// Gets or sets the hashed password.
        /// </summary>
        /// <value>
        /// The hashed password.
        /// </value>
        public string? HashedPassword { get; set; }

        /// <summary>
        /// Gets or sets the date and time at which the user last changed their password.
        /// </summary>
        /// <value>
        /// The date and time at which the user last changed their password.
        /// </value>
        public DateTime? PasswordCreated { get; set; }

        /// <summary>
        /// Gets or sets the security stamp.
        /// </summary>
        /// <remarks>
        /// This property contains an opaque string that changes whenever the user's existing
        /// sessions need to be invalidate.
        /// </remarks>
        /// <value>
        /// The security stamp.
        /// </value>
        public string? SecurityStamp { get; set; }

        /// <summary>
        /// Gets or sets the TZ ID of the user's time zone.
        /// </summary>
        /// <value>
        /// The TZ ID of the user's time zone.
        /// </value>
        public string? TimeZone { get; set; }

        /// <summary>
        /// Gets or sets the date and time at which the record was created.
        /// </summary>
        /// <value>
        /// The date and time at which the record was created.
        /// </value>
        public DateTime Created { get; set; }

        /// <summary>
        /// Gets or sets the date and time at which the record was last modified.
        /// </summary>
        /// <value>
        /// The date and time at which the record was last modified.
        /// </value>
        public DateTime Modified { get; set; }

        /// <summary>
        /// Gets or sets the revision number.
        /// </summary>
        /// <value>
        /// The revision number.
        /// </value>
        public int Revision { get; set; }
    }
}
