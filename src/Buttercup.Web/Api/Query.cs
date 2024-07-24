using System.Security.Claims;
using Buttercup.EntityModel;
using Buttercup.Security;
using Buttercup.Web.Security;
using HotChocolate.Authorization;

namespace Buttercup.Web.Api;

[QueryType]
public sealed class Query
{
    [UseSingleOrDefault]
    [UseProjection]
    public IQueryable<User>? CurrentUser(AppDbContext dbContext, ClaimsPrincipal principal)
    {
        var userId = principal.TryGetUserId();

        return userId.HasValue ? dbContext.Users.Where(u => u.Id == userId) : null;
    }

    [Authorize(Policy = AuthorizationPolicyNames.AdminOnly)]
    [UseProjection]
    public IQueryable<Recipe> DeletedRecipes(AppDbContext dbContext) =>
        dbContext.Recipes.WhereSoftDeleted();

    [Authorize]
    [UseSingleOrDefault]
    [UseProjection]
    public IQueryable<Recipe> Recipe(AppDbContext dbContext, long id) =>
        dbContext.Recipes.Where(r => r.Id == id);

    [Authorize]
    [UseProjection]
    public IQueryable<Recipe> Recipes(AppDbContext dbContext) =>
        dbContext.Recipes.WhereNotSoftDeleted();

    [Authorize]
    [UseSingleOrDefault]
    [UseProjection]
    public IQueryable<User> User(AppDbContext dbContext, long id) =>
        dbContext.Users.Where(u => u.Id == id);

    [Authorize]
    [UseProjection]
    public IQueryable<User> Users(AppDbContext dbContext) => dbContext.Users;
}
