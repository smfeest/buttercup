using System.Net;
using Buttercup.EntityModel;
using Microsoft.EntityFrameworkCore;

namespace Buttercup.Application;

internal sealed class RecipeManager(
    IDbContextFactory<AppDbContext> dbContextFactory, TimeProvider timeProvider)
    : IRecipeManager
{
    private readonly TimeProvider timeProvider = timeProvider;
    private readonly IDbContextFactory<AppDbContext> dbContextFactory = dbContextFactory;

    public async Task<long> CreateRecipe(
        RecipeAttributes attributes,
        long currentUserId,
        IPAddress? ipAddress,
        CancellationToken cancellationToken)
    {
        var timestamp = this.timeProvider.GetUtcDateTimeNow();
        var recipe = new Recipe()
        {
            Title = attributes.Title,
            PreparationMinutes = attributes.PreparationMinutes,
            CookingMinutes = attributes.CookingMinutes,
            Servings = attributes.Servings,
            Ingredients = attributes.Ingredients,
            Method = attributes.Method,
            Suggestions = attributes.Suggestions,
            Remarks = attributes.Remarks,
            Source = attributes.Source,
            Created = timestamp,
            CreatedByUserId = currentUserId,
            Modified = timestamp,
            ModifiedByUserId = currentUserId
        };

        recipe.Audits.Add(
            new()
            {
                Time = timestamp,
                Action = RecipeAction.Create,
                Revision = CreateRevision(recipe),
                ActorId = currentUserId,
                IpAddress = ipAddress,
            });

        using var dbContext = this.dbContextFactory.CreateDbContext();
        dbContext.Recipes.Add(recipe);
        await dbContext.SaveChangesAsync(cancellationToken);

        return recipe.Id;
    }

    public async Task<bool> DeleteRecipe(
        long id, long currentUserId, IPAddress? ipAddress, CancellationToken cancellationToken)
    {
        var timestamp = this.timeProvider.GetUtcDateTimeNow();

        using var dbContext = this.dbContextFactory.CreateDbContext();

        await using var transaction =
            await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var updatedRows = await dbContext
            .Recipes
            .Where(r => r.Id == id)
            .WhereNotSoftDeleted()
            .ExecuteUpdateAsync(
                s => s
                .SetProperty(r => r.Deleted, timestamp)
                .SetProperty(r => r.DeletedByUserId, currentUserId)
                .SetProperty(r => r.UpdateCount, r => r.Revision + 1),
                cancellationToken);

        if (updatedRows == 0)
        {
            return false;
        }

        dbContext.RecipeAudits.Add(
            new()
            {
                RecipeId = id,
                Time = timestamp,
                Action = RecipeAction.Delete,
                ActorId = currentUserId,
                IpAddress = ipAddress,
            });

        await dbContext.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return true;
    }

    public async Task<bool> HardDeleteRecipe(long id, CancellationToken cancellationToken)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        return await dbContext.Recipes
            .Where(r => r.Id == id).ExecuteDeleteAsync(cancellationToken) != 0;
    }

    public async Task<bool> UpdateRecipe(
        long id,
        RecipeAttributes newAttributes,
        int baseRevision,
        long currentUserId,
        IPAddress? ipAddress,
        CancellationToken cancellationToken)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        var recipe = await dbContext.Recipes.AsTracking().GetAsync(id, cancellationToken);

        if (recipe.Deleted.HasValue)
        {
            throw new SoftDeletedException($"Cannot update soft-deleted recipe {id}");
        }
        if (newAttributes == new RecipeAttributes(recipe))
        {
            return false;
        }
        if (recipe.Revision != baseRevision)
        {
            throw new ConcurrencyException(
                $"Revision {baseRevision} does not match current revision {recipe.Revision}");
        }

        var timestamp = this.timeProvider.GetUtcDateTimeNow();

        recipe.Title = newAttributes.Title;
        recipe.PreparationMinutes = newAttributes.PreparationMinutes;
        recipe.CookingMinutes = newAttributes.CookingMinutes;
        recipe.Servings = newAttributes.Servings;
        recipe.Ingredients = newAttributes.Ingredients;
        recipe.Method = newAttributes.Method;
        recipe.Suggestions = newAttributes.Suggestions;
        recipe.Remarks = newAttributes.Remarks;
        recipe.Source = newAttributes.Source;
        recipe.Modified = timestamp;
        recipe.ModifiedByUserId = currentUserId;
        recipe.Revision++;
        recipe.UpdateCount = recipe.Revision;

        recipe.Audits.Add(
            new()
            {
                Time = timestamp,
                Action = RecipeAction.Update,
                Revision = CreateRevision(recipe),
                ActorId = currentUserId,
                IpAddress = ipAddress,
            });

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return await this.UpdateRecipe(
                id, newAttributes, baseRevision, currentUserId, ipAddress, cancellationToken);
        }

        return true;
    }

    private static RecipeRevision CreateRevision(Recipe recipe) => new()
    {
        Recipe = recipe,
        Title = recipe.Title,
        PreparationMinutes = recipe.PreparationMinutes,
        CookingMinutes = recipe.CookingMinutes,
        Servings = recipe.Servings,
        Ingredients = recipe.Ingredients,
        Method = recipe.Method,
        Suggestions = recipe.Suggestions,
        Remarks = recipe.Remarks,
        Source = recipe.Source,
    };
}
