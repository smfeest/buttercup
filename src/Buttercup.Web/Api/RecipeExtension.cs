using Buttercup.EntityModel;

namespace Buttercup.Web.Api;

[ExtendObjectType<Recipe>(
    IgnoreProperties = [
        nameof(Recipe.CreatedByUserId),
        nameof(Recipe.ModifiedByUserId),
        nameof(Recipe.DeletedByUserId),
    ])]
public static class RecipeExtension
{
}
