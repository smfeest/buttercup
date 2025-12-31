using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Buttercup.EntityModel;

/// <summary>
/// Represents a user.
/// </summary>
[Index(nameof(Name))]
[Index(nameof(Email), IsUnique = true)]
[Index(nameof(Created))]
[Index(nameof(Modified))]
public sealed record User : IEntityId
{
    /// <summary>
    /// Gets or sets the primary key of the user.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the user's name.
    /// </summary>
    [StringLength(250)]
    public required string Name { get; set; }

    /// <summary>
    /// Gets or sets the user's email address.
    /// </summary>
    [StringLength(250)]
    public required string Email { get; set; }

    /// <summary>
    /// Gets or sets the user's hashed password, or null if the user does not have a password set.
    /// </summary>
    [StringLength(250)]
    public string? HashedPassword { get; set; }

    /// <summary>
    /// Gets or sets the date and time at which the user last changed their password.
    /// </summary>
    public DateTime? PasswordCreated { get; set; }

    /// <summary>
    /// Gets or sets security stamp; an opaque string that changes whenever the user's existing
    /// sessions need to be invalidate.
    /// </summary>
    [Column(TypeName = "char")]
    [StringLength(8)]
    public required string SecurityStamp { get; set; }

    /// <summary>
    /// Gets or sets the user's time zone as a TZID (e.g. 'Europe/London').
    /// </summary>
    [StringLength(50)]
    public required string TimeZone { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the user is an administrator.
    /// </summary>
    public bool IsAdmin { get; set; }

    /// <summary>
    /// Gets or sets the date and time at which the user was created.
    /// </summary>
    public required DateTime Created { get; set; }

    /// <summary>
    /// Gets or sets the date and time at which the user was last modified.
    /// </summary>
    public required DateTime Modified { get; set; }

    /// <summary>
    /// Gets or sets the date and time at which the user was deactivated, or null if the user is
    /// still active.
    /// </summary>
    public DateTime? Deactivated { get; set; }

    /// <summary>
    /// Gets or sets the revision number for concurrency control.
    /// </summary>
    [ConcurrencyCheck]
    public int Revision { get; set; }
}
