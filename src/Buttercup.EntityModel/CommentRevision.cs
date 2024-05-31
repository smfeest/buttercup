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

    /// <summary>
    /// Creates a new <see cref="CommentRevision" /> copying property values from a <see
    /// cref="Comment" />.
    /// </summary>
    /// <param name="comment">
    /// The comment to copy property values from.
    /// </param>
    /// <returns>
    /// The new <see cref="CommentRevision"/>.
    /// </returns>
    public static CommentRevision From(Comment comment) => new()
    {
        Comment = comment,
        CommentId = comment.Id,
        Revision = comment.Revision,
        Created = comment.Modified,
        Body = comment.Body,
    };
}
