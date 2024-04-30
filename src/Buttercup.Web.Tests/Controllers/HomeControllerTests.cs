using Buttercup.TestUtils;
using Buttercup.Web.Controllers.Queries;
using Buttercup.Web.Models.Home;
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

        var dbContextFactory = new FakeDbContextFactory();
        var queriesMock = new Mock<IHomeControllerQueries>();

        queriesMock
            .Setup(x => x.GetRecentlyAddedRecipes(dbContextFactory.FakeDbContext))
            .ReturnsAsync(recentlyAddedRecipes);
        queriesMock
            .Setup(
                x => x.GetRecentlyUpdatedRecipes(dbContextFactory.FakeDbContext, recentlyAddedIds))
            .ReturnsAsync(recentlyUpdatedRecipes);

        using var homeController = new HomeController(dbContextFactory, queriesMock.Object);

        var result = await homeController.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var viewModel = Assert.IsType<HomePageViewModel>(viewResult.Model);

        Assert.Same(recentlyAddedRecipes, viewModel.RecentlyAddedRecipes);
        Assert.Same(recentlyUpdatedRecipes, viewModel.RecentlyUpdatedRecipes);
    }

    #endregion
}
