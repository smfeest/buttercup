using Buttercup.EntityModel;

namespace Buttercup.Web.Api;

public sealed class CreateCommentPayload(long recipeId)
{
    /// <summary>
    /// The comment.
    /// </summary>
    [UseSingleOrDefault]
    [UseProjection]
    public IQueryable<Comment> Comment(AppDbContext dbContext) =>
        dbContext.Comments.Where(r => r.Id == recipeId);
}
