using System.Net;
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
    /// <param name="ipAddress">
    /// The IP address of the current user.
    /// </param>
    /// <param name="cancellationToken">
    /// The cancellation token.
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
    Task<long> CreateComment(
        long recipeId,
        CommentAttributes attributes,
        long currentUserId,
        IPAddress? ipAddress,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft-deletes a comment.
    /// </summary>
    /// <param name="id">
    /// The comment ID.
    /// </param>
    /// <param name="currentUserId">
    /// The current user ID.
    /// </param>
    /// <param name="ipAddress">
    /// The IP address of the current user.
    /// </param>
    /// <param name="cancellationToken">
    /// The cancellation token.
    /// </param>
    /// <returns>
    /// A task for the operation. The task result is <b>true</b> on success, <b>false</b> if the
    /// comment does not exist or has already been soft-deleted.
    /// </returns>
    Task<bool> DeleteComment(
        long id,
        long currentUserId,
        IPAddress? ipAddress,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Hard-deletes a comment.
    /// </summary>
    /// <param name="id">
    /// The comment ID.
    /// </param>
    /// <param name="cancellationToken">
    /// The cancellation token.
    /// </param>
    /// <returns>
    /// A task for the operation. The task result is <b>true</b> on success, <b>false</b> if the
    /// comment does not exist.
    /// </returns>
    Task<bool> HardDeleteComment(long id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a comment.
    /// </summary>
    /// <param name="id">
    /// The comment ID.
    /// </param>
    /// <param name="newAttributes">
    /// The new comment attributes.
    /// </param>
    /// <param name="baseUpdateCount">
    /// The base update count.
    /// </param>
    /// <param name="currentUserId">
    /// The current user ID.
    /// </param>
    /// <param name="ipAddress">
    /// The IP address of the current user.
    /// </param>
    /// <param name="cancellationToken">
    /// The cancellation token.
    /// </param>
    /// <returns>
    /// A task for the operation. The task result is <b>true</b> if the comment was updated,
    /// <b>false</b> if the comment's attributes already matched <paramref name="newAttributes"/>.
    /// </returns>
    /// <exception cref="NotFoundException">
    /// No matching comment was found.
    /// </exception>
    /// <exception cref="SoftDeletedException">
    /// Comment or recipe is soft-deleted.
    /// </exception>
    /// <exception cref="ConcurrencyException">
    /// <paramref name="baseUpdateCount"/> does not match the current update count in the database.
    /// </exception>
    Task<bool> UpdateComment(
        long id,
        CommentAttributes newAttributes,
        int baseUpdateCount,
        long currentUserId,
        IPAddress? ipAddress,
        CancellationToken cancellationToken = default);
}
