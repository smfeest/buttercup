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
        RecipeAttributes attributes, long currentUserId, CancellationToken cancellationToken)
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

        recipe.Audits.Add(new RecipeAudit
        {
            Time = timestamp,
            Action = RecipeAction.Create,
            ActorId = currentUserId,
            Changes = new RecipeChanges
            {
                Title = new(recipe.Title),
                PreparationMinutes = new(recipe.PreparationMinutes)
            }
        });

        recipe.Revisions.Add(RecipeRevision.From(recipe));

        using var dbContext = this.dbContextFactory.CreateDbContext();
        dbContext.Recipes.Add(recipe);
        await dbContext.SaveChangesAsync(cancellationToken);

        return recipe.Id;
    }

    public async Task<bool> DeleteRecipe(
        long id, long currentUserId, CancellationToken cancellationToken)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        var updatedRows = await dbContext
            .Recipes
            .Where(r => r.Id == id)
            .WhereNotSoftDeleted()
            .ExecuteUpdateAsync(
                s => s
                .SetProperty(r => r.Deleted, this.timeProvider.GetUtcDateTimeNow())
                .SetProperty(r => r.DeletedByUserId, currentUserId),
                cancellationToken);

        return updatedRows > 0;
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

        var audit = new RecipeAudit
        {
            Time = this.timeProvider.GetUtcDateTimeNow(),
            Action = RecipeAction.Modify,
            ActorId = currentUserId,
            Changes = new()
        };

        if (recipe.Title != newAttributes.Title)
        {
            audit.Changes.Title = new(recipe.Title, newAttributes.Title);
        }
        if (recipe.PreparationMinutes != newAttributes.PreparationMinutes)
        {
            audit.Changes.PreparationMinutes = new(recipe.PreparationMinutes, newAttributes.PreparationMinutes);
        }

        recipe.Title = newAttributes.Title;
        recipe.PreparationMinutes = newAttributes.PreparationMinutes;
        recipe.CookingMinutes = newAttributes.CookingMinutes;
        recipe.Servings = newAttributes.Servings;
        recipe.Ingredients = newAttributes.Ingredients;
        recipe.Method = newAttributes.Method;
        recipe.Suggestions = newAttributes.Suggestions;
        recipe.Remarks = newAttributes.Remarks;
        recipe.Source = newAttributes.Source;
        recipe.Modified = this.timeProvider.GetUtcDateTimeNow();
        recipe.ModifiedByUserId = currentUserId;
        recipe.Revision++;

        recipe.Audits.Add(audit);
        recipe.Revisions.Add(RecipeRevision.From(recipe));

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return await this.UpdateRecipe(
                id, newAttributes, baseRevision, currentUserId, cancellationToken);
        }

        return true;
    }
}
