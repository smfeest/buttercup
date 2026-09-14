using System.Net;
using Buttercup.EntityModel;
using Microsoft.EntityFrameworkCore;

namespace Buttercup.Application;

internal sealed class CommentManager(
    IDbContextFactory<AppDbContext> dbContextFactory, TimeProvider timeProvider)
    : ICommentManager
{
    private readonly TimeProvider timeProvider = timeProvider;
    private readonly IDbContextFactory<AppDbContext> dbContextFactory = dbContextFactory;

    public async Task<long> CreateComment(
        long recipeId,
        CommentAttributes attributes,
        long currentUserId,
        IPAddress? ipAddress,
        CancellationToken cancellationToken)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        var recipe = await dbContext.Recipes.GetAsync(recipeId, cancellationToken);

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

        comment.Audits.Add(
            new()
            {
                Time = timestamp,
                Action = CommentAction.Create,
                Revision = CreateRevision(comment),
                ActorId = currentUserId,
                IpAddress = ipAddress,
            });

        dbContext.Comments.Add(comment);
        await dbContext.SaveChangesAsync(cancellationToken);

        return comment.Id;
    }

    public async Task<bool> DeleteComment(
        long id, long currentUserId, IPAddress? ipAddress, CancellationToken cancellationToken)
    {
        var timestamp = this.timeProvider.GetUtcDateTimeNow();

        using var dbContext = this.dbContextFactory.CreateDbContext();

        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var updatedRows = await dbContext
            .Comments
            .Where(c => c.Id == id)
            .WhereNotSoftDeleted()
            .ExecuteUpdateAsync(
                s => s
                    .SetProperty(c => c.Deleted, timestamp)
                    .SetProperty(c => c.DeletedByUserId, currentUserId)
                    .SetProperty(c => c.UpdateCount, c => c.UpdateCount + 1),
                cancellationToken);

        if (updatedRows == 0)
        {
            return false;
        }

        dbContext.CommentAudits.Add(
            new()
            {
                CommentId = id,
                Time = timestamp,
                Action = CommentAction.Delete,
                ActorId = currentUserId,
                IpAddress = ipAddress,
            });

        await dbContext.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return true;
    }

    public async Task<bool> HardDeleteComment(long id, CancellationToken cancellationToken)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        return await dbContext
            .Comments
            .Where(r => r.Id == id)
            .ExecuteDeleteAsync(cancellationToken) != 0;
    }

    public async Task<bool> UpdateComment(
        long id,
        CommentAttributes newAttributes,
        int baseUpdateCount,
        long currentUserId,
        IPAddress? ipAddress,
        CancellationToken cancellationToken)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        var comment = await dbContext
            .Comments
            .Include(c => c.Recipe)
            .AsTracking()
            .GetAsync(id, cancellationToken);

        if (comment.Recipe!.Deleted.HasValue)
        {
            throw new SoftDeletedException($"Cannot update comment {id} on soft-deleted recipe {comment.RecipeId}");
        }
        if (comment.Deleted.HasValue)
        {
            throw new SoftDeletedException($"Cannot update soft-deleted comment {id}");
        }
        if (newAttributes == new CommentAttributes(comment))
        {
            return false;
        }
        if (comment.UpdateCount != baseUpdateCount)
        {
            throw new ConcurrencyException(
                $"Base update count {baseUpdateCount} does not match current update count {comment.UpdateCount}");
        }

        var timestamp = this.timeProvider.GetUtcDateTimeNow();

        comment.Body = newAttributes.Body;
        comment.Modified = timestamp;
        comment.UpdateCount++;

        comment.Audits.Add(
            new()
            {
                Time = timestamp,
                Action = CommentAction.Update,
                Revision = CreateRevision(comment),
                ActorId = currentUserId,
                IpAddress = ipAddress,
            });

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return await this.UpdateComment(
                id, newAttributes, baseUpdateCount, currentUserId, ipAddress, cancellationToken);
        }

        return true;
    }

    private static CommentRevision CreateRevision(Comment comment) => new()
    {
        Comment = comment,
        Body = comment.Body,
    };
}
