using Buttercup.Models;
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

        Assert.Equal(recipe.Id, editModel.Id);
        Assert.Equal(new(recipe), editModel.Attributes);
        Assert.Equal(recipe.Revision, editModel.Revision);
    }

    #endregion

    #region ToRecipe

    [Fact]
    public void ToRecipePopulatesAndReturnsNewRecipeModel()
    {
        var editModel = new RecipeEditModel(ModelFactory.CreateRecipe());

        var recipe = editModel.ToRecipe();

        Assert.Equal(editModel.Id, recipe.Id);
        Assert.Equal(editModel.Attributes, new(recipe));
        Assert.Equal(editModel.Revision, recipe.Revision);
    }

    #endregion
}
