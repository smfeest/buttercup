namespace Buttercup.EntityModel;

/// <summary>
/// Specifies the type of action performed on a comment.
/// </summary>
public enum CommentAction
{
    /// <summary>
    /// Indicates that the comment was created.
    /// </summary>
    Create,

    /// <summary>
    /// Indicates that the comment was soft-deleted.
    /// </summary>
    Delete,
}
