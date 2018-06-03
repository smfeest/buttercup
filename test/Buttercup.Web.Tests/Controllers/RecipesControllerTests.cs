using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Buttercup.DataAccess;
using Buttercup.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Buttercup.Web.Controllers
{
    public class RecipesControllerTests
    {
        #region Index

        [Fact]
        public async Task IndexReturnsViewResultWithRecipes()
        {
            using (var context = new Context())
            {
                IList<Recipe> recipes = Array.Empty<Recipe>();

                context.MockRecipeDataProvider
                    .Setup(x => x.GetRecipes(context.MockConnection.Object))
                    .ReturnsAsync(recipes);

                var result = await context.RecipesController.Index();
                var viewResult = Assert.IsType<ViewResult>(result);

                Assert.Same(recipes, viewResult.Model);
            }
        }

        #endregion

        #region Show

        [Fact]
        public async Task ShowReturnsViewResultWithRecipe()
        {
            using (var context = new Context())
            {
                var recipe = new Recipe();

                context.MockRecipeDataProvider
                    .Setup(x => x.GetRecipe(context.MockConnection.Object, 3))
                    .ReturnsAsync(recipe);

                var result = await context.RecipesController.Show(3);
                var viewResult = Assert.IsType<ViewResult>(result);

                Assert.Same(recipe, viewResult.Model);
            }
        }

        #endregion

        private class Context : IDisposable
        {
            public Context()
            {
                this.RecipesController = new RecipesController(
                    this.MockDbConnectionSource.Object, this.MockRecipeDataProvider.Object);

                this.MockDbConnectionSource
                    .Setup(x => x.OpenConnection())
                    .ReturnsAsync(this.MockConnection.Object);
            }

            public RecipesController RecipesController { get; }

            public Mock<DbConnection> MockConnection { get; } = new Mock<DbConnection>();

            public Mock<IDbConnectionSource> MockDbConnectionSource { get; } =
                new Mock<IDbConnectionSource>();

            public Mock<IRecipeDataProvider> MockRecipeDataProvider { get; } =
                new Mock<IRecipeDataProvider>();

            public void Dispose()
            {
                if (this.RecipesController != null)
                {
                    this.RecipesController.Dispose();
                }
            }
        }
    }
}
