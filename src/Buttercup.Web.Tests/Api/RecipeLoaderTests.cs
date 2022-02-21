using Buttercup.DataAccess;
using Buttercup.Models;
using Buttercup.TestUtils;
using GreenDonut;
using Moq;
using MySqlConnector;
using Xunit;

namespace Buttercup.Web.Api;

public class RecipeLoaderTests
{
    [Fact]
    public async void FetchesAndCachesRecipesById()
    {
        var mySqlConnection = new MySqlConnection();
        var mySqlConnectionSource = Mock.Of<IMySqlConnectionSource>(
            x => x.OpenConnection() == Task.FromResult(mySqlConnection));

        IList<Recipe> recipes = new[] { ModelFactory.CreateRecipe(), ModelFactory.CreateRecipe() };
        var recipeIds = new[] { recipes[0].Id, recipes[1].Id };

        var recipeDataProvider = Mock.Of<IRecipeDataProvider>(
            x => x.GetRecipes(mySqlConnection, recipeIds) == Task.FromResult(recipes),
            MockBehavior.Strict);

        using var recipeLoader = new RecipeLoader(
            mySqlConnectionSource, recipeDataProvider, new AutoBatchScheduler(), null);

        Assert.Equal(recipes, await recipeLoader.LoadAsync(new[] { recipes[0].Id, recipes[1].Id }));
        Assert.Equal(recipes[0], await recipeLoader.LoadAsync(recipes[0].Id));
    }
}
