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
            using (var context = new Context())
            {
                IList<Recipe> recentlyAddedRecipes = new[] { new Recipe() };
                IList<Recipe> recentlyUpdatedRecipes = new[] { new Recipe() };

                context.MockRecipeDataProvider
                    .Setup(x => x.GetRecentlyAddedRecipes(context.MockConnection.Object))
                    .ReturnsAsync(recentlyAddedRecipes);
                context.MockRecipeDataProvider
                    .Setup(x => x.GetRecentlyUpdatedRecipes(context.MockConnection.Object))
                    .ReturnsAsync(recentlyUpdatedRecipes);

                var result = await context.HomeController.Index();

                var viewResult = Assert.IsType<ViewResult>(result);
                var viewModel = Assert.IsType<HomePageViewModel>(viewResult.Model);

                Assert.Same(recentlyAddedRecipes, viewModel.RecentlyAddedRecipes);
                Assert.Same(recentlyUpdatedRecipes, viewModel.RecentlyUpdatedRecipes);
            }
        }

        #endregion

        private class Context : IDisposable
        {
            public Context()
            {
                this.HomeController = new HomeController(
                    this.MockDbConnectionSource.Object, this.MockRecipeDataProvider.Object);

                this.MockDbConnectionSource
                    .Setup(x => x.OpenConnection())
                    .ReturnsAsync(this.MockConnection.Object);
            }

            public HomeController HomeController { get; }

            public Mock<DbConnection> MockConnection { get; } = new Mock<DbConnection>();

            public Mock<IDbConnectionSource> MockDbConnectionSource { get; } =
                new Mock<IDbConnectionSource>();

            public Mock<IRecipeDataProvider> MockRecipeDataProvider { get; } =
                new Mock<IRecipeDataProvider>();

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
