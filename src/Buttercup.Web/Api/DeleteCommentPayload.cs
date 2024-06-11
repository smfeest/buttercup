using Buttercup.EntityModel;

namespace Buttercup.Web.Api;

public sealed class DeleteCommentPayload(long commentId, bool deleted)
{
    /// <summary>
    /// <b>true</b> if the comment was soft-deleted; <b>false</b> if the comment had already been
    /// soft-deleted.
    /// </summary>
    public bool Deleted => deleted;

    /// <summary>
    /// The deleted comment.
    /// </summary>
    [UseSingleOrDefault]
    [UseProjection]
    public IQueryable<Comment> Comment(AppDbContext dbContext) =>
        dbContext.Comments.Where(r => r.Id == commentId);
}
