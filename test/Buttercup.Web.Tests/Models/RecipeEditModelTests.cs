using Xunit;

namespace Buttercup.Web.Models
{
    public class RecipeEditModelTests
    {
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
        }

        #endregion
    }
}
