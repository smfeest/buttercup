using Buttercup.EntityModel;

namespace Buttercup.Application;

/// <summary>
/// Defines the contract for the comment manager.
/// </summary>
public interface ICommentManager
{
    /// <summary>
    /// Adds a new comment.
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
    Task<long> AddComment(long recipeId, CommentAttributes attributes, long currentUserId);
}
