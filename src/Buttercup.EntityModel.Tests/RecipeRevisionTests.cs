using Buttercup.TestUtils;
using Xunit;

namespace Buttercup.EntityModel;

public sealed class RecipeRevisionTests
{
    #region From

    [Fact]
    public void From_CopiesValuesFromRecipe()
    {
        var recipe = new ModelFactory().BuildRecipe(setOptionalAttributes: true);
        var expected = new RecipeRevision
        {
            Recipe = recipe,
            RecipeId = recipe.Id,
            Revision = recipe.Revision,
            Created = recipe.Modified,
            CreatedByUser = recipe.ModifiedByUser,
            CreatedByUserId = recipe.ModifiedByUserId,
            Title = recipe.Title,
            PreparationMinutes = recipe.PreparationMinutes,
            CookingMinutes = recipe.CookingMinutes,
            Servings = recipe.Servings,
            Ingredients = recipe.Ingredients,
            Method = recipe.Method,
            Suggestions = recipe.Suggestions,
            Remarks = recipe.Remarks,
            Source = recipe.Source,
        };
        var actual = RecipeRevision.From(recipe);
        Assert.Equal(expected, actual);
    }

    #endregion
}
