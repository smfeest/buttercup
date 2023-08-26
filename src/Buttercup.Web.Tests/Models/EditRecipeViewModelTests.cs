using Buttercup.TestUtils;
using Xunit;

namespace Buttercup.Web.Models;

public sealed class EditRecipeViewModelTests
{
    #region ForRecipe

    [Fact]
    public void ForRecipe_CopiesValuesFromRecipe()
    {
        var recipe = new ModelFactory().BuildRecipe();

        var editModel = EditRecipeViewModel.ForRecipe(recipe);

        Assert.Equal(recipe.Id, editModel.Id);
        Assert.Equal(new(recipe), editModel.Attributes);
        Assert.Equal(recipe.Revision, editModel.BaseRevision);
    }

    #endregion
}
