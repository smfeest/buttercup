using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Buttercup.EntityModel;

/// <summary>
/// Represents a comment.
/// </summary>
[Index(nameof(Deleted))]
public sealed record Comment : IEntityId, ISoftDeletable
{
    /// <summary>
    /// Gets or sets the primary key of the comment.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the recipe.
    /// </summary>
    public Recipe? Recipe { get; set; }

    /// <summary>
    /// Gets or sets the primary key of the recipe.
    /// </summary>
    public long RecipeId { get; set; }

    /// <summary>
    /// Gets or sets the user who added the comment.
    /// </summary>
    public User? Author { get; set; }

    /// <summary>
    /// Gets or sets the primary key of the user who added the comment.
    /// </summary>
    public long? AuthorId { get; set; }

    /// <summary>
    /// Gets or sets the comment body.
    /// </summary>
    [Column(TypeName = "text")]
    public required string Body { get; set; }

    /// <summary>
    /// Gets or sets the date and time at which the comment was added.
    /// </summary>
    public required DateTime Created { get; set; }

    /// <summary>
    /// Gets or sets the date and time at which the comment was last modified.
    /// </summary>
    public required DateTime Modified { get; set; }

    /// <summary>
    /// Gets or sets the date and time at which the comment was soft-deleted, or null if the comment
    /// has not been soft-deleted.
    /// </summary>
    [ConcurrencyCheck]
    public DateTime? Deleted { get; set; }

    /// <summary>
    /// Gets or sets the user who soft-deleted the comment.
    /// </summary>
    public User? DeletedByUser { get; set; }

    /// <summary>
    /// Gets or sets the primary key of the user who soft-deleted the comment.
    /// </summary>
    public long? DeletedByUserId { get; set; }

    /// <summary>
    /// Gets or sets the revision number for concurrency control.
    /// </summary>
    [ConcurrencyCheck]
    public int Revision { get; set; }

    /// <summary>
    /// Gets or sets the comment's revisions.
    /// </summary>
    public ICollection<CommentRevision> Revisions { get; set; } = [];
}
