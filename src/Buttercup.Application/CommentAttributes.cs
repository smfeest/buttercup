using System.ComponentModel.DataAnnotations;
using Buttercup.EntityModel;

namespace Buttercup.Application;

/// <summary>
/// Represents a comment's attributes.
/// </summary>
public sealed record CommentAttributes
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CommentAttributes" /> class.
    /// </summary>
    public CommentAttributes()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CommentAttributes" /> class with the attribute
    /// values from a comment.
    /// </summary>
    /// <param name="comment">
    /// The comment.
    /// </param>
    public CommentAttributes(Comment comment) =>
        this.Body = comment.Body;

    /// <summary>
    /// Gets or sets the comment body.
    /// </summary>
    /// <value>
    /// The comment body.
    /// </value>
    [Required(ErrorMessage = "Error_BodyRequired")]
    [StringLength(32000, ErrorMessage = "Error_BodyTooLong")]
    public string Body { get; init; } = string.Empty;
}
