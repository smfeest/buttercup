using Buttercup.TestUtils;
using Xunit;

namespace Buttercup.Web.Models;

public class EditRecipeViewModelTests
{
    #region ForRecipe

    [Fact]
    public void ForRecipeCopiesValuesFromRecipe()
    {
        var recipe = new ModelFactory().CreateRecipe();

        var editModel = EditRecipeViewModel.ForRecipe(recipe);

        Assert.Equal(recipe.Id, editModel.Id);
        Assert.Equal(new(recipe), editModel.Attributes);
        Assert.Equal(recipe.Revision, editModel.BaseRevision);
    }

    #endregion
}
