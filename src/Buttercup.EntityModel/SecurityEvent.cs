using System.ComponentModel.DataAnnotations;

namespace Buttercup.EntityModel;

/// <summary>
/// Represents a logged security event.
/// </summary>
public sealed record SecurityEvent
{
    /// <summary>
    /// Gets or sets the primary key of the event.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the timestamp.
    /// </summary>
    public required DateTime Time { get; set; }

    /// <summary>
    /// Gets or sets the event name.
    /// </summary>
    [StringLength(50)]
    public required string Event { get; set; }

    /// <summary>
    /// Gets or sets the associated user, if applicable.
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// Gets or sets the primary key of the associated user, if applicable.
    /// </summary>
    public long? UserId { get; set; }

    /// <summary>
    /// Gets or sets the associated email address, if applicable.
    /// </summary>
    [StringLength(250)]
    public string? Email { get; set; }
}
