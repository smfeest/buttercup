using Buttercup.DataAccess;
using Buttercup.EntityModel;
using Buttercup.TestUtils;
using Buttercup.Web.Authentication;
using Buttercup.Web.Models;
using Buttercup.Web.TestUtils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using MySqlConnector;
using Xunit;

namespace Buttercup.Web.Controllers;

public class RecipesControllerTests
{
    private readonly ModelFactory modelFactory = new();

    #region Index

    [Fact]
    public async Task IndexReturnsViewResultWithRecipes()
    {
        using var fixture = new RecipesControllerFixture();

        IList<Recipe> recipes = Array.Empty<Recipe>();

        fixture.MockRecipeDataProvider
            .Setup(x => x.GetAllRecipes(fixture.MySqlConnection))
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

        var recipe = this.modelFactory.BuildRecipe();

        fixture.MockRecipeDataProvider
            .Setup(x => x.GetRecipe(fixture.MySqlConnection, 3))
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
        using var fixture = new RecipesControllerFixture();

        var attributes = this.modelFactory.BuildRecipeAttributes();

        fixture.MockRecipeDataProvider
            .Setup(x => x.AddRecipe(fixture.MySqlConnection, attributes, fixture.User.Id))
            .ReturnsAsync(5);

        var result = await fixture.RecipesController.New(attributes);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(RecipesController.Show), redirectResult.ActionName);
        Assert.NotNull(redirectResult.RouteValues);
        Assert.Equal(5L, redirectResult.RouteValues["id"]);
    }

    [Fact]
    public async Task NewPostReturnsViewResultWithEditModelWhenModelIsInvalid()
    {
        using var fixture = new RecipesControllerFixture();

        var attributes = this.modelFactory.BuildRecipeAttributes();

        fixture.RecipesController.ModelState.AddModelError("test", "test");

        var result = await fixture.RecipesController.New(attributes);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(attributes, viewResult.Model);
    }

    #endregion

    #region Edit (GET)

    [Fact]
    public async Task EditGetReturnsViewResultWithEditModel()
    {
        using var fixture = new RecipesControllerFixture();

        var recipe = this.modelFactory.BuildRecipe();

        fixture.MockRecipeDataProvider
            .Setup(x => x.GetRecipe(fixture.MySqlConnection, 5))
            .ReturnsAsync(recipe);

        var result = await fixture.RecipesController.Edit(5);
        var viewResult = Assert.IsType<ViewResult>(result);

        var expectedModel = EditRecipeViewModel.ForRecipe(recipe);
        var actualModel = Assert.IsType<EditRecipeViewModel>(viewResult.Model);

        Assert.Equal(expectedModel, actualModel);
    }

    #endregion

    #region Edit (POST)

    [Fact]
    public async Task EditPostUpdatesRecipeAndRedirectsToShowPage()
    {
        using var fixture = new RecipesControllerFixture();

        var editModel = EditRecipeViewModel.ForRecipe(this.modelFactory.BuildRecipe());

        var result = await fixture.RecipesController.Edit(3, editModel);

        fixture.MockRecipeDataProvider.Verify(
            x => x.UpdateRecipe(
                fixture.MySqlConnection,
                3,
                editModel.Attributes,
                editModel.BaseRevision,
                fixture.User.Id));

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(RecipesController.Show), redirectResult.ActionName);
        Assert.NotNull(redirectResult.RouteValues);
        Assert.Equal(3L, redirectResult.RouteValues["id"]);
    }

    [Fact]
    public async Task EditPostReturnsViewResultWithEditModelWhenModelIsInvalid()
    {
        using var fixture = new RecipesControllerFixture();

        var editModel = EditRecipeViewModel.ForRecipe(this.modelFactory.BuildRecipe());

        fixture.RecipesController.ModelState.AddModelError("test", "test");

        var result = await fixture.RecipesController.Edit(3, editModel);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(editModel, viewResult.Model);
    }

    [Fact]
    public async Task EditPostReturnsViewResultAndAddsErrorWhenConcurrencyExceptionIsRaised()
    {
        using var fixture = new RecipesControllerFixture();

        var editModel = EditRecipeViewModel.ForRecipe(this.modelFactory.BuildRecipe());

        fixture.MockLocalizer.SetupLocalizedString(
            "Error_StaleEdit", "translated-stale-edit-error");

        fixture.MockRecipeDataProvider
            .Setup(x => x.UpdateRecipe(
                fixture.MySqlConnection,
                3,
                editModel.Attributes,
                editModel.BaseRevision,
                fixture.User.Id))
            .ThrowsAsync(new ConcurrencyException());

        var result = await fixture.RecipesController.Edit(3, editModel);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(editModel, viewResult.Model);

        var formState = fixture.RecipesController.ModelState[nameof(EditRecipeViewModel.Attributes)];
        Assert.NotNull(formState);

        var error = Assert.Single(formState.Errors);
        Assert.Equal("translated-stale-edit-error", error.ErrorMessage);
    }

    #endregion

    #region Delete (GET)

    [Fact]
    public async Task DeleteGetReturnsViewResultWithRecipe()
    {
        using var fixture = new RecipesControllerFixture();

        var recipe = this.modelFactory.BuildRecipe();

        fixture.MockRecipeDataProvider
            .Setup(x => x.GetRecipe(fixture.MySqlConnection, 8))
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
            .Setup(x => x.DeleteRecipe(fixture.MySqlConnection, 6))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var result = await fixture.RecipesController.DeletePost(6);

        fixture.MockRecipeDataProvider.Verify();

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(RecipesController.Index), redirectResult.ActionName);
    }

    #endregion

    private sealed class RecipesControllerFixture : IDisposable
    {
        public RecipesControllerFixture()
        {
            var mySqlConnectionSource = Mock.Of<IMySqlConnectionSource>(
                x => x.OpenConnection() == Task.FromResult(this.MySqlConnection));

            this.HttpContext.SetCurrentUser(this.User);

            this.RecipesController = new(
                this.MockLocalizer.Object,
                mySqlConnectionSource,
                this.MockRecipeDataProvider.Object)
            {
                ControllerContext = new() { HttpContext = this.HttpContext },
            };
        }

        public DefaultHttpContext HttpContext { get; } = new();

        public RecipesController RecipesController { get; }

        public MySqlConnection MySqlConnection { get; } = new();

        public User User { get; } = new ModelFactory().BuildUser();

        public Mock<IStringLocalizer<RecipesController>> MockLocalizer { get; } = new();

        public Mock<IRecipeDataProvider> MockRecipeDataProvider { get; } = new();

        public void Dispose() => this.RecipesController?.Dispose();
    }
}
