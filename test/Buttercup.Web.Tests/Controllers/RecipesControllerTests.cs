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
                    .Setup(x => x.GetRecipes(context.DbConnection))
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
                    .Setup(x => x.GetRecipe(context.DbConnection, 3))
                    .ReturnsAsync(recipe);

                var result = await context.RecipesController.Show(3);
                var viewResult = Assert.IsType<ViewResult>(result);

                Assert.Same(recipe, viewResult.Model);
            }
        }

        #endregion

        #region New (GET)

        [Fact]
        public void NewGetReturnsViewResult()
        {
            using (var context = new Context())
            {
                var result = context.RecipesController.New();
                var viewResult = Assert.IsType<ViewResult>(result);
            }
        }

        #endregion

        #region New (POST)

        [Fact]
        public async Task NewPostAddsRecipeAndRedirectsToShowPage()
        {
            using (var context = new Context())
            {
                var editModel = new RecipeEditModel
                {
                    Title = "recipe-title",
                };

                context.MockRecipeDataProvider
                    .Setup(x => x.AddRecipe(context.DbConnection, It.Is<Recipe>(
                        r => r.Title == editModel.Title && r.Created == context.UtcNow)))
                    .ReturnsAsync(5)
                    .Verifiable();

                var result = await context.RecipesController.New(editModel);

                context.MockRecipeDataProvider.Verify();

                var redirectResult = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal(nameof(RecipesController.Show), redirectResult.ActionName);
                Assert.Equal(5L, redirectResult.RouteValues["id"]);
            }
        }

        [Fact]
        public async Task NewPostReturnsViewResultWithEditModelWhenModelIsInvalid()
        {
            using (var context = new Context())
            {
                context.RecipesController.ModelState.AddModelError("test", "test");

                var editModel = new RecipeEditModel();
                var result = await context.RecipesController.New(editModel);

                var viewResult = Assert.IsType<ViewResult>(result);
                Assert.Same(editModel, viewResult.Model);
            }
        }

        #endregion

        #region Edit (GET)

        [Fact]
        public async Task EditGetReturnsViewResultWithEditModel()
        {
            using (var context = new Context())
            {
                var recipe = new Recipe
                {
                    Title = "recipe-title",
                };

                context.MockRecipeDataProvider
                    .Setup(x => x.GetRecipe(context.DbConnection, 5))
                    .ReturnsAsync(recipe);

                var result = await context.RecipesController.Edit(5);

                var viewResult = Assert.IsType<ViewResult>(result);
                var editModel = Assert.IsType<RecipeEditModel>(viewResult.Model);
                Assert.Equal(recipe.Title, editModel.Title);
            }
        }

        #endregion

        #region Edit (POST)

        [Fact]
        public async Task EditPostUpdatesRecipeAndRedirectsToShowPage()
        {
            using (var context = new Context())
            {
                var editModel = new RecipeEditModel
                {
                    Title = "recipe-title",
                };

                context.MockRecipeDataProvider
                    .Setup(x => x.UpdateRecipe(context.DbConnection, It.Is<Recipe>(
                        r => r.Id == 3 &&
                        r.Title == editModel.Title &&
                        r.Modified == context.UtcNow)))
                    .Returns(Task.CompletedTask)
                    .Verifiable();

                var result = await context.RecipesController.Edit(3, editModel);

                context.MockRecipeDataProvider.Verify();

                var redirectResult = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal(nameof(RecipesController.Show), redirectResult.ActionName);
                Assert.Equal(3L, redirectResult.RouteValues["id"]);
            }
        }

        [Fact]
        public async Task EditPostReturnsViewResultWithEditModelWhenModelIsInvalid()
        {
            using (var context = new Context())
            {
                context.RecipesController.ModelState.AddModelError("test", "test");

                var editModel = new RecipeEditModel();

                var result = await context.RecipesController.Edit(3, editModel);

                var viewResult = Assert.IsType<ViewResult>(result);
                Assert.Same(editModel, viewResult.Model);
            }
        }

        #endregion

        #region Delete (GET)

        [Fact]
        public async Task DeleteGetReturnsViewResultWithRecipe()
        {
            using (var context = new Context())
            {
                var recipe = new Recipe();

                context.MockRecipeDataProvider
                    .Setup(x => x.GetRecipe(context.DbConnection, 8))
                    .ReturnsAsync(recipe);

                var result = await context.RecipesController.Delete(8);

                var viewResult = Assert.IsType<ViewResult>(result);
                Assert.Same(recipe, viewResult.Model);
            }
        }

        #endregion

        #region Delete (POST)

        [Fact]
        public async Task DeletePostDeletesRecipeAndRedirectsToIndexPage()
        {
            using (var context = new Context())
            {
                context.MockRecipeDataProvider
                    .Setup(x => x.DeleteRecipe(context.DbConnection, 6, 12))
                    .Returns(Task.CompletedTask)
                    .Verifiable();

                var result = await context.RecipesController.Delete(6, 12);

                context.MockRecipeDataProvider.Verify();

                var redirectResult = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal(nameof(RecipesController.Index), redirectResult.ActionName);
            }
        }

        #endregion

        private class Context : IDisposable
        {
            public Context()
            {
                this.RecipesController = new RecipesController(
                    this.MockClock.Object,
                    this.MockDbConnectionSource.Object,
                    this.MockRecipeDataProvider.Object);

                this.MockClock.SetupGet(x => x.UtcNow).Returns(this.UtcNow);

                this.MockDbConnectionSource
                    .Setup(x => x.OpenConnection())
                    .ReturnsAsync(this.DbConnection);
            }

            public RecipesController RecipesController { get; }

            public DbConnection DbConnection { get; } = Mock.Of<DbConnection>();

            public Mock<IClock> MockClock { get; } = new Mock<IClock>();

            public Mock<IDbConnectionSource> MockDbConnectionSource { get; } =
                new Mock<IDbConnectionSource>();

            public Mock<IRecipeDataProvider> MockRecipeDataProvider { get; } =
                new Mock<IRecipeDataProvider>();

            public DateTime UtcNow { get; } = new DateTime(2000, 1, 2, 3, 4, 5, DateTimeKind.Utc);

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
