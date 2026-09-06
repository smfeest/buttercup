using Buttercup.Application;
using Buttercup.EntityModel;

namespace Buttercup.Web.Models.Recipes;

public sealed record EditRecipeViewModel(long Id, RecipeAttributes Attributes, int BaseUpdateCount)
{
    public static EditRecipeViewModel ForRecipe(Recipe recipe) => new(
        recipe.Id, new(recipe), recipe.UpdateCount);
}
