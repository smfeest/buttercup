using Buttercup.DataAccess;
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
    public async Task IndexReturnsViewResultWithRecentlyAddedRecipes()
    {
        using var fixture = new HomeControllerFixture();

        var recentlyAddedRecipes = new[] { this.modelFactory.BuildRecipe() };
        var recentlyAddedIds = new[] { recentlyAddedRecipes[0].Id };
        var recentlyUpdatedRecipes = new[] { this.modelFactory.BuildRecipe() };

        fixture.MockRecipeDataProvider
            .Setup(x => x.GetRecentlyAddedRecipes(fixture.DbContextFactory.FakeDbContext))
            .ReturnsAsync(recentlyAddedRecipes);
        fixture.MockRecipeDataProvider
            .Setup(x => x.GetRecentlyUpdatedRecipes(
                fixture.DbContextFactory.FakeDbContext, recentlyAddedIds))
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
            this.HomeController = new(this.DbContextFactory, this.MockRecipeDataProvider.Object);

        public FakeDbContextFactory DbContextFactory { get; } = new();

        public HomeController HomeController { get; }

        public Mock<IRecipeDataProvider> MockRecipeDataProvider { get; } = new();

        public void Dispose()
        {
            this.DbContextFactory.Dispose();
            this.HomeController.Dispose();
        }
    }
}
