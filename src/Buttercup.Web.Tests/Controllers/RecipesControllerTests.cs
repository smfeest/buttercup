using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading.Tasks;
using Buttercup.DataAccess;
using Buttercup.Models;
using Buttercup.Web.Authentication;
using Buttercup.Web.Models;
using Microsoft.AspNetCore.Http;
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
            using var fixture = new RecipesControllerFixture();

            IList<Recipe> recipes = Array.Empty<Recipe>();

            fixture.MockRecipeDataProvider
                .Setup(x => x.GetRecipes(fixture.DbConnection))
                .ReturnsAsync(recipes);

            var result = await fixture.RecipesController.Index();
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Same(recipes, viewResult.Model);
        }

        #endregion

        #region Show

        [Fact]
        public async Task ShowReturnsViewResultWithRecipe()
        {
            using var fixture = new RecipesControllerFixture();

            var recipe = new Recipe();

            fixture.MockRecipeDataProvider
                .Setup(x => x.GetRecipe(fixture.DbConnection, 3))
                .ReturnsAsync(recipe);

            var result = await fixture.RecipesController.Show(3);
            var viewResult = Assert.IsType<ViewResult>(result);

            Assert.Same(recipe, viewResult.Model);
        }

        #endregion

        #region New (GET)

        [Fact]
        public void NewGetReturnsViewResult()
        {
            using var fixture = new RecipesControllerFixture();

            var result = fixture.RecipesController.New();
            var viewResult = Assert.IsType<ViewResult>(result);
        }

        #endregion

        #region New (POST)

        [Fact]
        public async Task NewPostAddsRecipeAndRedirectsToShowPage()
        {
            using var fixture = new NewEditPostFixture();

            fixture.MockRecipeDataProvider
                .Setup(x => x.AddRecipe(fixture.DbConnection, It.IsAny<Recipe>()))
                .Callback((DbConnection connection, Recipe recipe) =>
                {
                    Assert.Equal(fixture.EditModel.Title, recipe.Title);
                    Assert.Equal(fixture.UtcNow, recipe.Created);
                    Assert.Equal(fixture.User.Id, recipe.CreatedByUserId);
                })
                .ReturnsAsync(5)
                .Verifiable();

            var result = await fixture.RecipesController.New(fixture.EditModel);

            fixture.MockRecipeDataProvider.Verify();

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(RecipesController.Show), redirectResult.ActionName);
            Assert.Equal(5L, redirectResult.RouteValues["id"]);
        }

        [Fact]
        public async Task NewPostReturnsViewResultWithEditModelWhenModelIsInvalid()
        {
            using var fixture = new NewEditPostFixture();

            fixture.RecipesController.ModelState.AddModelError("test", "test");

            var result = await fixture.RecipesController.New(fixture.EditModel);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Same(fixture.EditModel, viewResult.Model);
        }

        #endregion

        #region Edit (GET)

        [Fact]
        public async Task EditGetReturnsViewResultWithEditModel()
        {
            using var fixture = new RecipesControllerFixture();

            var recipe = new Recipe
            {
                Title = "recipe-title",
            };

            fixture.MockRecipeDataProvider
                .Setup(x => x.GetRecipe(fixture.DbConnection, 5))
                .ReturnsAsync(recipe);

            var result = await fixture.RecipesController.Edit(5);

            var viewResult = Assert.IsType<ViewResult>(result);
            var editModel = Assert.IsType<RecipeEditModel>(viewResult.Model);
            Assert.Equal(recipe.Title, editModel.Title);
        }

        #endregion

        #region Edit (POST)

        [Fact]
        public async Task EditPostUpdatesRecipeAndRedirectsToShowPage()
        {
            using var fixture = new NewEditPostFixture();

            fixture.MockRecipeDataProvider
                .Setup(x => x.UpdateRecipe(fixture.DbConnection, It.IsAny<Recipe>()))
                .Callback((DbConnection connection, Recipe recipe) =>
                {
                    Assert.Equal(3, recipe.Id);
                    Assert.Equal(fixture.EditModel.Title, recipe.Title);
                    Assert.Equal(fixture.UtcNow, recipe.Modified);
                    Assert.Equal(fixture.User.Id, recipe.ModifiedByUserId);
                })
                .Returns(Task.CompletedTask)
                .Verifiable();

            var result = await fixture.RecipesController.Edit(3, fixture.EditModel);

            fixture.MockRecipeDataProvider.Verify();

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(RecipesController.Show), redirectResult.ActionName);
            Assert.Equal(3L, redirectResult.RouteValues["id"]);
        }

        [Fact]
        public async Task EditPostReturnsViewResultWithEditModelWhenModelIsInvalid()
        {
            using var fixture = new NewEditPostFixture();

            fixture.RecipesController.ModelState.AddModelError("test", "test");

            var result = await fixture.RecipesController.Edit(3, fixture.EditModel);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Same(fixture.EditModel, viewResult.Model);
        }

        #endregion

        #region Delete (GET)

        [Fact]
        public async Task DeleteGetReturnsViewResultWithRecipe()
        {
            using var fixture = new RecipesControllerFixture();

            var recipe = new Recipe();

            fixture.MockRecipeDataProvider
                .Setup(x => x.GetRecipe(fixture.DbConnection, 8))
                .ReturnsAsync(recipe);

            var result = await fixture.RecipesController.Delete(8);

            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Same(recipe, viewResult.Model);
        }

        #endregion

        #region Delete (POST)

        [Fact]
        public async Task DeletePostDeletesRecipeAndRedirectsToIndexPage()
        {
            using var fixture = new RecipesControllerFixture();

            fixture.MockRecipeDataProvider
                .Setup(x => x.DeleteRecipe(fixture.DbConnection, 6, 12))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var result = await fixture.RecipesController.Delete(6, 12);

            fixture.MockRecipeDataProvider.Verify();

            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(nameof(RecipesController.Index), redirectResult.ActionName);
        }

        #endregion

        private class RecipesControllerFixture : IDisposable
        {
            public RecipesControllerFixture()
            {
                var clock = Mock.Of<IClock>(x => x.UtcNow == this.UtcNow);
                var dbConnectionSource = Mock.Of<IDbConnectionSource>(
                    x => x.OpenConnection() == Task.FromResult(this.DbConnection));

                this.RecipesController = new(
                    clock, dbConnectionSource, this.MockRecipeDataProvider.Object);
            }

            public RecipesController RecipesController { get; }

            public DbConnection DbConnection { get; } = Mock.Of<DbConnection>();

            public Mock<IRecipeDataProvider> MockRecipeDataProvider { get; } = new();

            public DateTime UtcNow { get; } = new(2000, 1, 2, 3, 4, 5, DateTimeKind.Utc);

            public void Dispose()
            {
                if (this.RecipesController != null)
                {
                    this.RecipesController.Dispose();
                }
            }
        }

        private class NewEditPostFixture : RecipesControllerFixture
        {
            public NewEditPostFixture()
            {
                this.HttpContext.SetCurrentUser(this.User);

                this.RecipesController.ControllerContext = new()
                {
                    HttpContext = this.HttpContext,
                };
            }

            public DefaultHttpContext HttpContext { get; } = new();

            public RecipeEditModel EditModel { get; } = new() { Title = "recipe-title" };

            public User User { get; } = new() { Id = 8 };
        }
    }
}