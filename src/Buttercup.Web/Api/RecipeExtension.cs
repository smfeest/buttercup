using Buttercup.EntityModel;

namespace Buttercup.Web.Api;

[ExtendObjectType<Recipe>]
public static class RecipeExtension
{
    [UseProjection]
    public static IQueryable<Comment> Comments(AppDbContext dbContext, [Parent] Recipe recipe) =>
        dbContext.Comments.WhereNotSoftDeleted().Where(c => c.RecipeId == recipe.Id);
}
