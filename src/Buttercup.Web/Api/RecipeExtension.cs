using Buttercup.EntityModel;

namespace Buttercup.Web.Api;

[ExtendObjectType<Recipe>(
    IgnoreProperties = new[] { nameof(Recipe.CreatedByUserId), nameof(Recipe.ModifiedByUserId) })]
public class RecipeExtension
{
}
