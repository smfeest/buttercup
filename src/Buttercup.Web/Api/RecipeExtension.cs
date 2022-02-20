using Buttercup.Models;

namespace Buttercup.Web.Api;

[ExtendObjectType(typeof(Recipe))]
public class RecipeExtension
{
    [BindMember(nameof(Recipe.CreatedByUserId))]
    public Task<User?> CreatedByUser([Parent] Recipe recipe, IUserLoader userLoader) =>
        LoadUserOrNull(recipe.CreatedByUserId, userLoader);

    [BindMember(nameof(Recipe.ModifiedByUserId))]
    public Task<User?> ModifiedByUser([Parent] Recipe recipe, IUserLoader userLoader) =>
        LoadUserOrNull(recipe.ModifiedByUserId, userLoader);

    private static async Task<User?> LoadUserOrNull(long? userId, IUserLoader userLoader) =>
        userId.HasValue ? await userLoader.LoadAsync(userId.Value) : null;
}
