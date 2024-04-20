using System.ComponentModel.DataAnnotations;

namespace Buttercup.Application;

/// <summary>
/// Represents a comment's attributes.
/// </summary>
public sealed record CommentAttributes
{
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
