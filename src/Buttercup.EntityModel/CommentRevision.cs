using System.ComponentModel.DataAnnotations.Schema;

namespace Buttercup.EntityModel;

/// <summary>
/// Represents a reversion of a comment.
/// </summary>
public sealed record CommentRevision
{
    /// <summary>
    /// Gets or sets the primary key of the revision.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the comment body.
    /// </summary>
    [Column(TypeName = "text")]
    public required string Body { get; set; }
}
