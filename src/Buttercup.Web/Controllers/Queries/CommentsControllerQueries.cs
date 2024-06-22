using Buttercup.EntityModel;
using Microsoft.EntityFrameworkCore;

namespace Buttercup.Web.Controllers.Queries;

public sealed class CommentsControllerQueries : ICommentsControllerQueries
{
    public Task<Comment?> FindComment(AppDbContext dbContext, long id) =>
        dbContext.Comments.WhereNotSoftDeleted().FindAsync(id);

    public Task<Comment?> FindCommentWithAuthor(AppDbContext dbContext, long id) =>
        dbContext.Comments.WhereNotSoftDeleted().Include(c => c.Author).FindAsync(id);
}
