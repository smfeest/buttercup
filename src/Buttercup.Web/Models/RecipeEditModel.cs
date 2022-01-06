using Buttercup.Models;

namespace Buttercup.Web.Models;

public sealed record RecipeEditModel(long Id, RecipeAttributes Attributes, int Revision)
{
    public Recipe ToRecipe() => new(
        this.Id,
        this.Attributes.Title,
        this.Attributes.PreparationMinutes,
        this.Attributes.CookingMinutes,
        this.Attributes.Servings,
        this.Attributes.Ingredients,
        this.Attributes.Method,
        this.Attributes.Suggestions,
        this.Attributes.Remarks,
        this.Attributes.Source,
        new(),
        null,
        new(),
        null,
        this.Revision);

    public static RecipeEditModel ForRecipe(Recipe recipe) => new(
        recipe.Id, new(recipe), recipe.Revision);
}
