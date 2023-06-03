using System.Security.Claims;
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

    [UseSingleOrDefault]
    [UseProjection]
    public IQueryable<User>? CurrentUser(AppDbContext dbContext, ClaimsPrincipal principal)
    {
        var userId = principal.GetUserId();

        return userId.HasValue ? dbContext.Users.Where(u => u.Id == userId) : null;
    }

    [Authorize]
    [UseSingleOrDefault]
    [UseProjection]
    public IQueryable<Recipe> Recipe(AppDbContext dbContext, long id) =>
        dbContext.Recipes.Where(r => r.Id == id);

    [Authorize]
    [UseProjection]
    public IQueryable<Recipe> Recipes(AppDbContext dbContext) => dbContext.Recipes;
}
