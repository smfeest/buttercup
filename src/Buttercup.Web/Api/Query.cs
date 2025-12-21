using System.Security.Claims;
using Buttercup.EntityModel;
using Buttercup.Security;
using Buttercup.Web.Security;
using HotChocolate.Authorization;

namespace Buttercup.Web.Api;

[QueryType]
public sealed class Query
{
    [Authorize(
        AuthorizationPolicyNames.AuthenticatedAndAdminWhenDeleted, ApplyPolicy.AfterResolver)]
    [UseSingleOrDefault]
    [UseProjection]
    public IQueryable<Comment> Comment(AppDbContext dbContext, long id) =>
        dbContext.Comments.Where(c => c.Id == id).OrderBy(c => c.Id);

    [Authorize]
    [Authorize(AuthorizationPolicyNames.AdminOnlyFilterAndSortFields)]
    [UsePaging]
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

        return userId.HasValue ?
            dbContext.Users.Where(u => u.Id == userId).OrderBy(u => u.Id) :
            null;
    }

    [Authorize(AuthorizationPolicyNames.AdminOnly)]
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseTieBreakSortById<Comment>]
    [UseSorting]
    public IQueryable<Comment> DeletedComments(AppDbContext dbContext) =>
        dbContext.Comments.WhereSoftDeleted();

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
        dbContext.Recipes.Where(r => r.Id == id).OrderBy(r => r.Id);

    [Authorize]
    [Authorize(AuthorizationPolicyNames.AdminOnlyFilterAndSortFields)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Recipe> Recipes(AppDbContext dbContext) =>
        dbContext.Recipes.WhereNotSoftDeleted();

    [Authorize(AuthorizationPolicyNames.AdminOnly)]
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseTieBreakSortById<SecurityEvent>]
    [UseSorting]
    public IQueryable<SecurityEvent> SecurityEvents(AppDbContext dbContext) =>
        dbContext.SecurityEvents;

    [Authorize(AuthorizationPolicyNames.AdminOnly)]
    [UsePaging]
    [UseProjection]
    [UseFiltering]
    [UseTieBreakSortById<UserAuditEntry>]
    [UseSorting]
    public IQueryable<UserAuditEntry> UserAuditEntries(AppDbContext dbContext) =>
        dbContext.UserAuditEntries;

    [Authorize]
    [UseSingleOrDefault]
    [UseProjection]
    public IQueryable<User> User(AppDbContext dbContext, long id) =>
        dbContext.Users.Where(u => u.Id == id).OrderBy(u => u.Id);

    [Authorize]
    [Authorize(AuthorizationPolicyNames.AdminOnlyFilterAndSortFields)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<User> Users(AppDbContext dbContext) => dbContext.Users;
}
