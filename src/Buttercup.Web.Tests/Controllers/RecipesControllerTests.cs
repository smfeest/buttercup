using Buttercup.Application;
using Buttercup.EntityModel;
using Buttercup.TestUtils;
using Buttercup.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Moq;
using Xunit;

namespace Buttercup.Web.Controllers;

public sealed class RecipesControllerTests
{
    private readonly ModelFactory modelFactory = new();

    #region Index

    [Fact]
    public async Task Index_ReturnsViewResultWithRecipes()
    {
        using var fixture = new RecipesControllerFixture();

        IList<Recipe> recipes = Array.Empty<Recipe>();

        fixture.MockRecipeManager.Setup(x => x.GetAllRecipes()).ReturnsAsync(recipes);

        var result = await fixture.RecipesController.Index();
        var viewResult = Assert.IsType<ViewResult>(result);

        Assert.Same(recipes, viewResult.Model);
    }

    #endregion

    #region Show

    [Fact]
    public async Task Show_ReturnsViewResultWithRecipe()
    {
        using var fixture = new RecipesControllerFixture();

        var recipe = this.modelFactory.BuildRecipe();

        fixture.MockRecipeManager.Setup(x => x.GetRecipe(3)).ReturnsAsync(recipe);

        var result = await fixture.RecipesController.Show(3);
        var viewResult = Assert.IsType<ViewResult>(result);

        Assert.Same(recipe, viewResult.Model);
    }

    #endregion

    #region New (GET)

    [Fact]
    public void New_Get_ReturnsViewResult()
    {
        using var fixture = new RecipesControllerFixture();

        var result = fixture.RecipesController.New();
        var viewResult = Assert.IsType<ViewResult>(result);
    }

    #endregion

    #region New (POST)

    [Fact]
    public async Task New_Post_Success_AddsRecipeAndRedirectsToShowPage()
    {
        using var fixture = new RecipesControllerFixture();

        var attributes = this.modelFactory.BuildRecipeAttributes();

        fixture.MockRecipeManager
            .Setup(x => x.AddRecipe(attributes, fixture.CurrentUserId))
            .ReturnsAsync(5);

        var result = await fixture.RecipesController.New(attributes);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(RecipesController.Show), redirectResult.ActionName);
        Assert.NotNull(redirectResult.RouteValues);
        Assert.Equal(5L, redirectResult.RouteValues["id"]);
    }

    [Fact]
    public async Task New_Post_InvalidModel_ReturnsViewResultWithEditModel()
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
    public async Task Edit_Get_ReturnsViewResultWithEditModel()
    {
        using var fixture = new RecipesControllerFixture();

        var recipe = this.modelFactory.BuildRecipe();

        fixture.MockRecipeManager.Setup(x => x.GetRecipe(5)).ReturnsAsync(recipe);

        var result = await fixture.RecipesController.Edit(5);
        var viewResult = Assert.IsType<ViewResult>(result);

        var expectedModel = EditRecipeViewModel.ForRecipe(recipe);
        var actualModel = Assert.IsType<EditRecipeViewModel>(viewResult.Model);

        Assert.Equal(expectedModel, actualModel);
    }

    #endregion

    #region Edit (POST)

    [Fact]
    public async Task Edit_Post_Success_UpdatesRecipeAndRedirectsToShowPage()
    {
        using var fixture = new RecipesControllerFixture();

        var editModel = EditRecipeViewModel.ForRecipe(this.modelFactory.BuildRecipe());

        var result = await fixture.RecipesController.Edit(3, editModel);

        fixture.MockRecipeManager.Verify(
            x => x.UpdateRecipe(
                3, editModel.Attributes, editModel.BaseRevision, fixture.CurrentUserId));

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(RecipesController.Show), redirectResult.ActionName);
        Assert.NotNull(redirectResult.RouteValues);
        Assert.Equal(3L, redirectResult.RouteValues["id"]);
    }

    [Fact]
    public async Task Edit_Post_InvalidModel_ReturnsViewResultWithEditModel()
    {
        using var fixture = new RecipesControllerFixture();

        var editModel = EditRecipeViewModel.ForRecipe(this.modelFactory.BuildRecipe());

        fixture.RecipesController.ModelState.AddModelError("test", "test");

        var result = await fixture.RecipesController.Edit(3, editModel);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(editModel, viewResult.Model);
    }

    [Fact]
    public async Task Edit_Post_ConcurrencyException_ReturnsViewResultAndAddsError()
    {
        using var fixture = new RecipesControllerFixture();

        var editModel = EditRecipeViewModel.ForRecipe(this.modelFactory.BuildRecipe());

        fixture.MockLocalizer.SetupLocalizedString(
            "Error_StaleEdit", "translated-stale-edit-error");

        fixture.MockRecipeManager
            .Setup(x => x.UpdateRecipe(
                3, editModel.Attributes, editModel.BaseRevision, fixture.CurrentUserId))
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
    public async Task Delete_Get_ReturnsViewResultWithRecipe()
    {
        using var fixture = new RecipesControllerFixture();

        var recipe = this.modelFactory.BuildRecipe();

        fixture.MockRecipeManager.Setup(x => x.GetRecipe(8)).ReturnsAsync(recipe);

        var result = await fixture.RecipesController.Delete(8);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(recipe, viewResult.Model);
    }

    #endregion

    #region Delete (POST)

    [Fact]
    public async Task Delete_Post_DeletesRecipeAndRedirectsToIndexPage()
    {
        using var fixture = new RecipesControllerFixture();

        fixture.MockRecipeManager
            .Setup(x => x.DeleteRecipe(6))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var result = await fixture.RecipesController.DeletePost(6);

        fixture.MockRecipeManager.Verify();

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(RecipesController.Index), redirectResult.ActionName);
    }

    #endregion

    private sealed class RecipesControllerFixture : IDisposable
    {
        public RecipesControllerFixture()
        {
            this.HttpContext.User = PrincipalFactory.CreateWithUserId(this.CurrentUserId);

            this.RecipesController = new(
                this.MockLocalizer.Object,
                this.MockRecipeManager.Object)
            {
                ControllerContext = new() { HttpContext = this.HttpContext },
            };
        }

        public DefaultHttpContext HttpContext { get; } = new();

        public RecipesController RecipesController { get; }

        public long CurrentUserId { get; } = new ModelFactory().NextInt();

        public Mock<IStringLocalizer<RecipesController>> MockLocalizer { get; } = new();

        public Mock<IRecipeManager> MockRecipeManager { get; } = new();

        public void Dispose() => this.RecipesController.Dispose();
    }
}
