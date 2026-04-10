using Buttercup.EntityModel;
using Microsoft.EntityFrameworkCore;

namespace Buttercup.Web.Controllers.Queries;

public sealed class CommentsControllerQueries : ICommentsControllerQueries
{
    public Task<Comment?> FindComment(
        AppDbContext dbContext, long id, CancellationToken cancellationToken) =>
        dbContext.Comments.WhereNotSoftDeleted().FindAsync(id, cancellationToken);

    public Task<Comment?> FindCommentWithAuthor(
        AppDbContext dbContext, long id, CancellationToken cancellationToken) =>
        dbContext.Comments
            .WhereNotSoftDeleted()
            .Include(c => c.Author)
            .FindAsync(id, cancellationToken);
}
