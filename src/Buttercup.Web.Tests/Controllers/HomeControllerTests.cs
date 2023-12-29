using Buttercup.Application;
using Buttercup.TestUtils;
using Buttercup.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Buttercup.Web.Controllers;

public sealed class HomeControllerTests
{
    #region Index

    [Fact]
    public async Task Index_ReturnsViewResultWithRecentlyAddedAndUpdatedRecipes()
    {
        var modelFactory = new ModelFactory();

        var recentlyAddedRecipes = new[] { modelFactory.BuildRecipe() };
        var recentlyAddedIds = new[] { recentlyAddedRecipes[0].Id };
        var recentlyUpdatedRecipes = new[] { modelFactory.BuildRecipe() };

        var recipeManagerMock = new Mock<IRecipeManager>();

        recipeManagerMock
            .Setup(x => x.GetRecentlyAddedRecipes())
            .ReturnsAsync(recentlyAddedRecipes);
        recipeManagerMock
            .Setup(x => x.GetRecentlyUpdatedRecipes(recentlyAddedIds))
            .ReturnsAsync(recentlyUpdatedRecipes);

        using var homeController = new HomeController(recipeManagerMock.Object);

        var result = await homeController.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<HomePageViewModel>(viewResult.Model);

        Assert.Same(recentlyAddedRecipes, viewModel.RecentlyAddedRecipes);
        Assert.Same(recentlyUpdatedRecipes, viewModel.RecentlyUpdatedRecipes);
    }

    #endregion
}
