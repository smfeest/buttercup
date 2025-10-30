using Buttercup.EntityModel;

namespace Buttercup.Application;

/// <summary>
/// Defines the contract for the comment manager.
/// </summary>
public interface ICommentManager
{
    /// <summary>
    /// Creates a new comment.
    /// </summary>
    /// <param name="recipeId">
    /// The recipe ID.
    /// </param>
    /// <param name="attributes">
    /// The comment attributes.
    /// </param>
    /// <param name="currentUserId">
    /// The current user ID.
    /// </param>
    /// <returns>
    /// A task for the operation. The task result is the ID of the new comment.
    /// </returns>
    /// <exception cref="NotFoundException">
    /// No matching recipe was found.
    /// </exception>
    /// <exception cref="SoftDeletedException">
    /// Recipe is soft-deleted.
    /// </exception>
    Task<long> CreateComment(long recipeId, CommentAttributes attributes, long currentUserId);

    /// <summary>
    /// Soft-deletes a comment.
    /// </summary>
    /// <param name="id">
    /// The comment ID.
    /// </param>
    /// <param name="currentUserId">
    /// The current user ID.
    /// </param>
    /// <returns>
    /// A task for the operation. The task result is <b>true</b> on success, <b>false</b> if the
    /// comment does not exist or has already been soft-deleted.
    /// </returns>
    Task<bool> DeleteComment(long id, long currentUserId);

    /// <summary>
    /// Hard-deletes a comment.
    /// </summary>
    /// <param name="id">
    /// The comment ID.
    /// </param>
    /// <returns>
    /// A task for the operation. The task result is <b>true</b> on success, <b>false</b> if the
    /// comment does not exist.
    /// </returns>
    Task<bool> HardDeleteComment(long id);
}
