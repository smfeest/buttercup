using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Buttercup.DataAccess;
using Buttercup.Models;
using Buttercup.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Buttercup.Web.Controllers
{
    public class HomeControllerTests
    {
        #region Index

        [Fact]
        public async Task IndexReturnsViewResultWithRecentlyAddedRecipes()
        {
            using var fixture = new HomeControllerFixture();

            IList<Recipe> recentlyAddedRecipes = new[] { new Recipe() };
            IList<Recipe> recentlyUpdatedRecipes = new[] { new Recipe() };

            fixture.MockRecipeDataProvider
                .Setup(x => x.GetRecentlyAddedRecipes(fixture.DbConnection))
                .ReturnsAsync(recentlyAddedRecipes);
            fixture.MockRecipeDataProvider
                .Setup(x => x.GetRecentlyUpdatedRecipes(fixture.DbConnection))
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
                var dbConnectionSource = Mock.Of<IDbConnectionSource>(
                    x => x.OpenConnection() == Task.FromResult(this.DbConnection));

                this.HomeController = new(dbConnectionSource, this.MockRecipeDataProvider.Object);
            }

            public HomeController HomeController { get; }

            public DbConnection DbConnection { get; } = Mock.Of<DbConnection>();

            public Mock<IRecipeDataProvider> MockRecipeDataProvider { get; } = new();

            public void Dispose()
            {
                if (this.HomeController != null)
                {
                    this.HomeController.Dispose();
                }
            }
        }
    }
}
