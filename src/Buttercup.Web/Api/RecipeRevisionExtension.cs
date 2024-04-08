using Buttercup.EntityModel;

namespace Buttercup.Web.Api;

[ExtendObjectType<RecipeRevision>(
    IgnoreProperties = [nameof(RecipeRevision.RecipeId), nameof(RecipeRevision.CreatedByUserId)])]
public static class RecipeRevisionExtension
{
}
