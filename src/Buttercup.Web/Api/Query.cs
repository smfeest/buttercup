using System.Security.Claims;
using Buttercup.DataAccess;
using Buttercup.EntityModel;
using Buttercup.Web.Authentication;
using HotChocolate.Authorization;

namespace Buttercup.Web.Api;

[QueryType]
public sealed class Query
{
    private readonly IMySqlConnectionSource mySqlConnectionSource;

    public Query(IMySqlConnectionSource mySqlConnectionSource) =>
        this.mySqlConnectionSource = mySqlConnectionSource;

    public async Task<User?> CurrentUser(
        [Service] IUserDataProvider userDataProvider, ClaimsPrincipal principal)
    {
        var userId = principal.GetUserId();

        if (!userId.HasValue)
        {
            return null;
        }

        using var connection = await this.mySqlConnectionSource.OpenConnection();

        return await userDataProvider.GetUser(connection, userId.Value);
    }

    [Authorize]
    public Task<Recipe> Recipe(IRecipesByIdDataLoader recipeLoader, long id) =>
        recipeLoader.LoadAsync(id);

    [Authorize]
    public async Task<IList<Recipe>> Recipes([Service] IRecipeDataProvider recipeDataProvider)
    {
        using var connection = await this.mySqlConnectionSource.OpenConnection();

        return await recipeDataProvider.GetAllRecipes(connection);
    }
}
