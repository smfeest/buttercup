using Buttercup.EntityModel;
using Microsoft.EntityFrameworkCore;

namespace Buttercup.Application;

internal sealed class CommentManager(
    IDbContextFactory<AppDbContext> dbContextFactory, TimeProvider timeProvider)
    : ICommentManager
{
    private readonly TimeProvider timeProvider = timeProvider;
    private readonly IDbContextFactory<AppDbContext> dbContextFactory = dbContextFactory;

    public async Task<long> AddComment(
        long recipeId, CommentAttributes attributes, long currentUserId)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        var recipe = await dbContext.Recipes.GetAsync(recipeId);

        if (recipe.Deleted.HasValue)
        {
            throw new SoftDeletedException($"Cannot add comment to soft-deleted recipe {recipeId}");
        }

        var timestamp = this.timeProvider.GetUtcDateTimeNow();
        var comment = new Comment
        {
            RecipeId = recipeId,
            AuthorId = currentUserId,
            Body = attributes.Body,
            Created = timestamp,
            Modified = timestamp,
        };
        comment.Revisions.Add(CommentRevision.From(comment));

        dbContext.Comments.Add(comment);
        await dbContext.SaveChangesAsync();

        return comment.Id;
    }

    public async Task<bool> DeleteComment(long id, long currentUserId)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        var updatedRows = await dbContext
            .Comments
            .Where(c => c.Id == id)
            .WhereNotSoftDeleted()
            .ExecuteUpdateAsync(s => s
                .SetProperty(c => c.Deleted, this.timeProvider.GetUtcDateTimeNow())
                .SetProperty(c => c.DeletedByUserId, currentUserId));

        return updatedRows > 0;
    }

    public async Task<bool> HardDeleteComment(long id)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        return await dbContext.Comments.Where(r => r.Id == id).ExecuteDeleteAsync() != 0;
    }
}
