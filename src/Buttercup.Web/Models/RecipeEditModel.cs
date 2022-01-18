using Buttercup.Models;

namespace Buttercup.Web.Models;

public sealed record RecipeEditModel(long Id, RecipeAttributes Attributes, int Revision)
{
    public static RecipeEditModel ForRecipe(Recipe recipe) => new(
        recipe.Id, new(recipe), recipe.Revision);
}
