using Buttercup.DataAccess;
using Buttercup.EntityModel;
using Microsoft.EntityFrameworkCore;

namespace Buttercup.Web.Api;

[ExtendObjectType<Recipe>(
    IgnoreProperties = new[] { nameof(Recipe.CreatedByUserId), nameof(Recipe.ModifiedByUserId) })]
public class RecipeExtension
{
    [DataLoader]
    public static async Task<IReadOnlyDictionary<long, Recipe>> GetRecipesByIdAsync(
        IReadOnlyList<long> keys,
        IDbContextFactory<AppDbContext> dbContextFactory,
        IRecipeDataProvider recipeDataProvider)
    {
        using var dbContext = dbContextFactory.CreateDbContext();

        var recipes = await recipeDataProvider.GetRecipes(dbContext, keys);

        return recipes.ToDictionary(x => x.Id);
    }
}
