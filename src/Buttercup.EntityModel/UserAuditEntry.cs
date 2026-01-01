using System.Net;
using Microsoft.EntityFrameworkCore;

namespace Buttercup.EntityModel;

/// <summary>
/// Represents an entry in the user audit log.
/// </summary>
[Index(nameof(Time))]
public sealed record UserAuditEntry : IEntityId
{
    /// <summary>
    /// Gets or sets the primary key of the entry.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the timestamp.
    /// </summary>
    public DateTime Time { get; set; }

    /// <summary>
    /// Gets or sets the operation.
    /// </summary>
    public UserAuditOperation Operation { get; set; }

    /// <summary>
    /// Gets or sets the user that was the affected by the operation.
    /// </summary>
    public User? Target { get; set; }

    /// <summary>
    /// Gets or sets the primary key of the user that was the affected by the operation.
    /// </summary>
    public long TargetId { get; set; }

    /// <summary>
    /// Gets or sets the user who initiated the operation.
    /// </summary>
    public User? Actor { get; set; }

    /// <summary>
    /// Gets or sets the primary key of the user who initiated the operation.
    /// </summary>
    public long ActorId { get; set; }

    /// <summary>
    /// Gets or sets the IP address of the client through which the operation was initiated.
    /// </summary>
    public IPAddress? IpAddress { get; set; }

    /// <summary>
    /// Gets or sets the reason for failure, if unsuccessful.
    /// </summary>
    public UserAuditFailure? Failure { get; set; }
}
