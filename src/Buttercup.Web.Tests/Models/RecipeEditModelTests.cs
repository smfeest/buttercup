using Buttercup.Models;
using Buttercup.TestUtils;
using Xunit;

namespace Buttercup.Web.Models;

public class RecipeEditModelTests
{
    #region ToRecipe

    [Fact]
    public void ToRecipePopulatesAndReturnsNewRecipeModel()
    {
        var editModel = RecipeEditModel.ForRecipe(ModelFactory.CreateRecipe());

        var recipe = editModel.ToRecipe();

        Assert.Equal(editModel.Id, recipe.Id);
        Assert.Equal(editModel.Attributes, new(recipe));
        Assert.Equal(editModel.Revision, recipe.Revision);
    }

    #endregion

    #region ForRecipe

    [Fact]
    public void ForRecipeCopiesValuesFromRecipe()
    {
        var recipe = ModelFactory.CreateRecipe();

        var editModel = RecipeEditModel.ForRecipe(recipe);

        Assert.Equal(recipe.Id, editModel.Id);
        Assert.Equal(new(recipe), editModel.Attributes);
        Assert.Equal(recipe.Revision, editModel.Revision);
    }

    #endregion
}
