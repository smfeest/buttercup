using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Buttercup.EntityModel;

/// <summary>
/// Represents a reversion of a comment.
/// </summary>
[PrimaryKey(nameof(CommentId), nameof(Revision))]
public sealed record CommentRevision
{
    /// <summary>
    /// Gets or sets the comment.
    /// </summary>
    public Comment? Comment { get; set; }

    /// <summary>
    /// Gets or sets the primary key of the comment.
    /// </summary>
    public long CommentId { get; set; }

    /// <summary>
    /// Gets or sets the revision number.
    /// </summary>
    public int Revision { get; set; }

    /// <summary>
    /// Gets or sets the date and time at which the revision was created.
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// Gets or sets the comment body.
    /// </summary>
    [Column(TypeName = "text")]
    public required string Body { get; set; }
}
