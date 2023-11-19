using Buttercup.EntityModel;

namespace Buttercup.Web.Api;

[ExtendObjectType<Recipe>(
    IgnoreProperties = [nameof(Recipe.CreatedByUserId), nameof(Recipe.ModifiedByUserId)])]
public static class RecipeExtension
{
}
