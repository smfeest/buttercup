using Buttercup.EntityModel;
using Buttercup.Web.Security;
using HotChocolate.Authorization;

namespace Buttercup.Web.Api;

[ExtendObjectType<Recipe>]
public static class RecipeExtension
{
    [UsePaging]
    [UseProjection]
    public static IQueryable<Comment> Comments(AppDbContext dbContext, [Parent] Recipe recipe) =>
        dbContext.Comments
            .WhereNotSoftDeleted()
            .Where(c => c.RecipeId == recipe.Id)
            .OrderBy(c => c.Id);

    [Authorize(Policy = AuthorizationPolicyNames.AdminOnly)]
    [UsePaging]
    [UseProjection]
    public static IQueryable<Comment> DeletedComments(
        AppDbContext dbContext, [Parent] Recipe recipe) =>
        dbContext.Comments
            .WhereSoftDeleted()
            .Where(c => c.RecipeId == recipe.Id)
            .OrderBy(c => c.Id);
}
