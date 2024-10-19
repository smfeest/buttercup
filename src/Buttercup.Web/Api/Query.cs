using System.Security.Claims;
using Buttercup.EntityModel;
using Buttercup.Security;
using Buttercup.Web.Security;
using HotChocolate.Authorization;

namespace Buttercup.Web.Api;

[QueryType]
public sealed class Query
{
    [Authorize]
    [Authorize(AuthorizationPolicyNames.AdminOnlyFilterAndSortFields)]
    [UsePaging(MaxPageSize = 500)]
    [UseProjection]
    [UseFiltering]
    [UseTieBreakSortById<Comment>]
    [UseSorting]
    public IQueryable<Comment> Comments(AppDbContext dbContext) =>
        dbContext.Comments.WhereNotSoftDeleted();

    [UseSingleOrDefault]
    [UseProjection]
    public IQueryable<User>? CurrentUser(AppDbContext dbContext, ClaimsPrincipal principal)
    {
        var userId = principal.TryGetUserId();

        return userId.HasValue ? dbContext.Users.Where(u => u.Id == userId) : null;
    }

    [Authorize(AuthorizationPolicyNames.AdminOnly)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Recipe> DeletedRecipes(AppDbContext dbContext) =>
        dbContext.Recipes.WhereSoftDeleted();

    [Authorize(AuthorizationPolicyNames.AuthenticatedAndAdminWhenDeleted, ApplyPolicy.AfterResolver)]
    [UseSingleOrDefault]
    [UseProjection]
    public IQueryable<Recipe> Recipe(AppDbContext dbContext, long id) =>
        dbContext.Recipes.Where(r => r.Id == id);

    [Authorize]
    [Authorize(AuthorizationPolicyNames.AdminOnlyFilterAndSortFields)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Recipe> Recipes(AppDbContext dbContext) =>
        dbContext.Recipes.WhereNotSoftDeleted();

    [Authorize]
    [UseSingleOrDefault]
    [UseProjection]
    public IQueryable<User> User(AppDbContext dbContext, long id) =>
        dbContext.Users.Where(u => u.Id == id);

    [Authorize]
    [Authorize(AuthorizationPolicyNames.AdminOnlyFilterAndSortFields)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<User> Users(AppDbContext dbContext) => dbContext.Users;
}
