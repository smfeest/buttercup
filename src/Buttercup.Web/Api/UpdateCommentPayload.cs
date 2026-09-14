using Buttercup.EntityModel;

namespace Buttercup.Web.Api;

public sealed class UpdateCommentPayload(long commentId)
{
    /// <summary>
    /// The updated comment.
    /// </summary>
    [UseSingleOrDefault]
    [UseProjection]
    public IQueryable<Comment> Comment(AppDbContext dbContext) =>
        dbContext.Comments.Where(c => c.Id == commentId).OrderBy(c => c.Id);
}
