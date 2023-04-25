namespace Buttercup.EntityModel;

/// <summary>
/// Represents a user.
/// </summary>
public sealed record User
{
    /// <summary>
    /// Gets or sets the primary key of the user.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the user's name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Gets or sets the user's hashed password, or null if the user does not have a password set.
    /// </summary>
    public string? HashedPassword { get; set; }

    /// <summary>
    /// Gets or sets the date and time at which the user last changed their password.
    /// </summary>
    public DateTime? PasswordCreated { get; set; }

    /// <summary>
    /// Gets or sets security stamp; an opaque string that changes whenever the user's existing
    /// sessions need to be invalidate.
    /// </summary>
    public required string SecurityStamp { get; set; }

    /// <summary>
    /// Gets or sets the user's time zone as a TZID (e.g. 'Europe/London').
    /// </summary>
    public required string TimeZone { get; set; }

    /// <summary>
    /// Gets or sets the date and time at which the user was created.
    /// </summary>
    public required DateTime Created { get; set; }

    /// <summary>
    /// Gets or sets the date and time at which the user was last modified.
    /// </summary>
    public required DateTime Modified { get; set; }

    /// <summary>
    /// Gets or sets the revision number for concurrency control.
    /// </summary>
    public int Revision { get; set; }
}
