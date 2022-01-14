using Buttercup.TestUtils;
using Xunit;

namespace Buttercup.Web.Models;

public class RecipeEditModelTests
{
    #region Constructor(Recipe)

    [Fact]
    public void ConstructorCopiesValuesFromRecipe()
    {
        var recipe = ModelFactory.CreateRecipe();

        var editModel = new RecipeEditModel(recipe);

        Assert.Equal(recipe.Title, editModel.Title);
        Assert.Equal(recipe.PreparationMinutes, editModel.PreparationMinutes);
        Assert.Equal(recipe.CookingMinutes, editModel.CookingMinutes);
        Assert.Equal(recipe.Servings, editModel.Servings);
        Assert.Equal(recipe.Ingredients, editModel.Ingredients);
        Assert.Equal(recipe.Method, editModel.Method);
        Assert.Equal(recipe.Suggestions, editModel.Suggestions);
        Assert.Equal(recipe.Remarks, editModel.Remarks);
        Assert.Equal(recipe.Source, editModel.Source);
        Assert.Equal(recipe.Revision, editModel.Revision);
    }

    #endregion

    #region ToRecipe

    [Fact]
    public void ToRecipePopulatesAndReturnsNewRecipeModel()
    {
        var editModel = new RecipeEditModel
        {
            Title = "recipe-title",
            PreparationMinutes = 1,
            CookingMinutes = 2,
            Servings = 3,
            Ingredients = "recipe-ingredients",
            Method = "recipe-method",
            Suggestions = "recipe-suggestions",
            Remarks = "recipe-remarks",
            Source = "recipe-source",
            Revision = 4,
        };

        var recipe = editModel.ToRecipe();

        Assert.Equal(editModel.Title, recipe.Title);
        Assert.Equal(editModel.PreparationMinutes, recipe.PreparationMinutes);
        Assert.Equal(editModel.CookingMinutes, recipe.CookingMinutes);
        Assert.Equal(editModel.Servings, recipe.Servings);
        Assert.Equal(editModel.Ingredients, recipe.Ingredients);
        Assert.Equal(editModel.Method, recipe.Method);
        Assert.Equal(editModel.Suggestions, recipe.Suggestions);
        Assert.Equal(editModel.Remarks, recipe.Remarks);
        Assert.Equal(editModel.Source, recipe.Source);
        Assert.Equal(editModel.Revision, recipe.Revision);
    }

    #endregion
}
