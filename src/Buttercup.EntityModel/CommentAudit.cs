using System.Net;

namespace Buttercup.EntityModel;

/// <summary>
/// Represents an audit entry for a comment.
/// </summary>
public sealed record CommentAudit
{
    /// <summary>
    /// Gets or sets the primary key of the audit entry.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the comment.
    /// </summary>
    public Comment? Comment { get; set; }

    /// <summary>
    /// Gets or sets the primary key of the comment.
    /// </summary>
    public long CommentId { get; set; }

    /// <summary>
    /// Gets or sets the date and time of the action.
    /// </summary>
    public DateTime Time { get; set; }

    /// <summary>
    /// Gets or sets the action.
    /// </summary>
    public CommentAction Action { get; set; }

    /// <summary>
    /// Gets or sets the primary key of the comment revision.
    /// </summary>
    /// <remarks>
    /// This property is specified if and only if <see cref="Action"/> is <see
    /// cref="CommentAction.Create"/>.
    /// </remarks>
    public CommentRevision? Revision { get; set; }

    /// <summary>
    /// Gets or sets the primary key of the comment revision.
    /// </summary>
    /// <remarks>
    /// This property is specified if and only if <see cref="Action"/> is <see
    /// cref="CommentAction.Create"/>.
    /// </remarks>
    public long? RevisionId { get; set; }

    /// <summary>
    /// Gets or sets the user that performed the action.
    /// </summary>
    public User? Actor { get; set; }

    /// <summary>
    /// Gets or sets the primary key of the user that performed the action.
    /// </summary>
    public long? ActorId { get; set; }

    /// <summary>
    /// Gets or sets the IP address of the user that performed the action.
    /// </summary>
    public IPAddress? IpAddress { get; set; }
}
