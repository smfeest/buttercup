using Buttercup.DataAccess;
using Buttercup.Models;
using Buttercup.TestUtils;
using Buttercup.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MySqlConnector;
using Xunit;

namespace Buttercup.Web.Controllers;

public class HomeControllerTests
{
    #region Index

    [Fact]
    public async Task IndexReturnsViewResultWithRecentlyAddedRecipes()
    {
        using var fixture = new HomeControllerFixture();

        IList<Recipe> recentlyAddedRecipes = new[] { ModelFactory.CreateRecipe() };
        IList<Recipe> recentlyUpdatedRecipes = new[] { ModelFactory.CreateRecipe() };

        fixture.MockRecipeDataProvider
            .Setup(x => x.GetRecentlyAddedRecipes(fixture.MySqlConnection))
            .ReturnsAsync(recentlyAddedRecipes);
        fixture.MockRecipeDataProvider
            .Setup(x => x.GetRecentlyUpdatedRecipes(fixture.MySqlConnection))
            .ReturnsAsync(recentlyUpdatedRecipes);

        var result = await fixture.HomeController.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<HomePageViewModel>(viewResult.Model);

        Assert.Same(recentlyAddedRecipes, viewModel.RecentlyAddedRecipes);
        Assert.Same(recentlyUpdatedRecipes, viewModel.RecentlyUpdatedRecipes);
    }

    #endregion

    private class HomeControllerFixture : IDisposable
    {
        public HomeControllerFixture()
        {
            var mySqlConnectionSource = Mock.Of<IMySqlConnectionSource>(
                x => x.OpenConnection() == Task.FromResult(this.MySqlConnection));

            this.HomeController = new(mySqlConnectionSource, this.MockRecipeDataProvider.Object);
        }

        public HomeController HomeController { get; }

        public MySqlConnection MySqlConnection { get; } = new();

        public Mock<IRecipeDataProvider> MockRecipeDataProvider { get; } = new();

        public void Dispose() => this.HomeController?.Dispose();
    }
}
