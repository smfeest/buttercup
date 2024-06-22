using Buttercup.EntityModel;

namespace Buttercup.Web.Controllers.Queries;

/// <summary>
/// Provides database queries for <see cref="CommentsController"/>.
/// </summary>
public interface ICommentsControllerQueries
{
    /// <summary>
    /// Finds a non-deleted a comment.
    /// </summary>
    /// <param name="dbContext">
    /// The database context.
    /// </param>
    /// <param name="id">
    /// The comment ID.
    /// </param>
    /// <returns>
    /// A task for the operation. The result is the comment, or null if the comment does not exist
    /// or is soft-deleted.
    /// </returns>
    Task<Comment?> FindComment(AppDbContext dbContext, long id);

    /// <summary>
    /// Finds a non-deleted a comment, loading the associated author.
    /// </summary>
    /// <param name="dbContext">
    /// The database context.
    /// </param>
    /// <param name="id">
    /// The comment ID.
    /// </param>
    /// <returns>
    /// A task for the operation. The result is the comment, or null if the comment does not exist
    /// or is soft-deleted.
    /// </returns>
    Task<Comment?> FindCommentWithAuthor(AppDbContext dbContext, long id);
}
