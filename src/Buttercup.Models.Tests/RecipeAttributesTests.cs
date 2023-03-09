using Buttercup.TestUtils;
using Xunit;

namespace Buttercup.Models;

public class RecipeAttributesTests
{
    #region Constructor(Recipe)

    [Fact]
    public void ConstructorCopiesValuesFromRecipe()
    {
        var recipe = new ModelFactory().BuildRecipe();

        var attributes = new RecipeAttributes(recipe);

        Assert.Equal(recipe.Title, attributes.Title);
        Assert.Equal(recipe.PreparationMinutes, attributes.PreparationMinutes);
        Assert.Equal(recipe.CookingMinutes, attributes.CookingMinutes);
        Assert.Equal(recipe.Servings, attributes.Servings);
        Assert.Equal(recipe.Ingredients, attributes.Ingredients);
        Assert.Equal(recipe.Method, attributes.Method);
        Assert.Equal(recipe.Suggestions, attributes.Suggestions);
        Assert.Equal(recipe.Remarks, attributes.Remarks);
        Assert.Equal(recipe.Source, attributes.Source);
    }

    #endregion
}
