using Buttercup.TestUtils;
using Xunit;

namespace Buttercup.EntityModel;

public sealed class RecipeAttributesTests
{
    #region Constructor(RecipeAttributes)

    [Fact]
    public void Constructor_CopiesValuesFromSource()
    {
        var source = new ModelFactory().BuildRecipe();

        var attributes = new RecipeAttributes(source);

        Assert.Equal(source.Title, attributes.Title);
        Assert.Equal(source.PreparationMinutes, attributes.PreparationMinutes);
        Assert.Equal(source.CookingMinutes, attributes.CookingMinutes);
        Assert.Equal(source.Servings, attributes.Servings);
        Assert.Equal(source.Ingredients, attributes.Ingredients);
        Assert.Equal(source.Method, attributes.Method);
        Assert.Equal(source.Suggestions, attributes.Suggestions);
        Assert.Equal(source.Remarks, attributes.Remarks);
        Assert.Equal(source.Source, attributes.Source);
    }

    #endregion
}
