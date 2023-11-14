using Buttercup.Application;
using Buttercup.TestUtils;
using Buttercup.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Buttercup.Web.Controllers;

public sealed class HomeControllerTests
{
    private readonly ModelFactory modelFactory = new();

    #region Index

    [Fact]
    public async Task Index_ReturnsViewResultWithRecentlyAddedRecipes()
    {
        using var fixture = new HomeControllerFixture();

        var recentlyAddedRecipes = new[] { this.modelFactory.BuildRecipe() };
        var recentlyAddedIds = new[] { recentlyAddedRecipes[0].Id };
        var recentlyUpdatedRecipes = new[] { this.modelFactory.BuildRecipe() };

        fixture.MockRecipeManager
            .Setup(x => x.GetRecentlyAddedRecipes())
            .ReturnsAsync(recentlyAddedRecipes);
        fixture.MockRecipeManager
            .Setup(x => x.GetRecentlyUpdatedRecipes(recentlyAddedIds))
            .ReturnsAsync(recentlyUpdatedRecipes);

        var result = await fixture.HomeController.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<HomePageViewModel>(viewResult.Model);

        Assert.Same(recentlyAddedRecipes, viewModel.RecentlyAddedRecipes);
        Assert.Same(recentlyUpdatedRecipes, viewModel.RecentlyUpdatedRecipes);
    }

    #endregion

    private sealed class HomeControllerFixture : IDisposable
    {
        public HomeControllerFixture() =>
            this.HomeController = new(this.MockRecipeManager.Object);

        public HomeController HomeController { get; }

        public Mock<IRecipeManager> MockRecipeManager { get; } = new();

        public void Dispose() => this.HomeController.Dispose();
    }
}
