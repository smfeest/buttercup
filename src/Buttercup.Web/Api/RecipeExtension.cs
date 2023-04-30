using Buttercup.DataAccess;
using Buttercup.EntityModel;

namespace Buttercup.Web.Api;

[ExtendObjectType<Recipe>(
    IgnoreProperties = new[] { nameof(Recipe.CreatedByUser), nameof(Recipe.ModifiedByUser) })]
public class RecipeExtension
{
    [BindMember(nameof(Recipe.CreatedByUserId))]
    public Task<User?> CreatedByUser([Parent] Recipe recipe, IUsersByIdDataLoader userLoader) =>
        LoadUserOrNull(recipe.CreatedByUserId, userLoader);

    [BindMember(nameof(Recipe.ModifiedByUserId))]
    public Task<User?> ModifiedByUser([Parent] Recipe recipe, IUsersByIdDataLoader userLoader) =>
        LoadUserOrNull(recipe.ModifiedByUserId, userLoader);

    [DataLoader]
    public static async Task<IReadOnlyDictionary<long, Recipe>> GetRecipesByIdAsync(
        IReadOnlyList<long> keys,
        IMySqlConnectionSource mySqlConnectionSource,
        IRecipeDataProvider recipeDataProvider)
    {
        using var connection = await mySqlConnectionSource.OpenConnection();

        var recipes = await recipeDataProvider.GetRecipes(connection, keys);

        return recipes.ToDictionary(x => x.Id);
    }

    private static async Task<User?> LoadUserOrNull(
        long? userId, IUsersByIdDataLoader userLoader) =>
        userId.HasValue ? await userLoader.LoadAsync(userId.Value) : null;
}
