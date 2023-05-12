using System.Security.Claims;
using Buttercup.DataAccess;
using Buttercup.EntityModel;
using Buttercup.Web.Authentication;
using HotChocolate.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Buttercup.Web.Api;

[QueryType]
public sealed class Query
{
    private readonly IDbContextFactory<AppDbContext> dbContextFactory;

    public Query(IDbContextFactory<AppDbContext> dbContextFactory) =>
        this.dbContextFactory = dbContextFactory;

    public async Task<User?> CurrentUser(
        [Service] IUserDataProvider userDataProvider, ClaimsPrincipal principal)
    {
        var userId = principal.GetUserId();

        if (!userId.HasValue)
        {
            return null;
        }

        using var dbContext = this.dbContextFactory.CreateDbContext();

        return await userDataProvider.GetUser(dbContext, userId.Value);
    }

    [Authorize]
    public Task<Recipe> Recipe(IRecipesByIdDataLoader recipeLoader, long id) =>
        recipeLoader.LoadAsync(id);

    [Authorize]
    public async Task<IList<Recipe>> Recipes([Service] IRecipeDataProvider recipeDataProvider)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        return await recipeDataProvider.GetAllRecipes(dbContext);
    }
}
