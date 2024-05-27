using Buttercup.Application;
using Buttercup.EntityModel;
using Buttercup.TestUtils;
using Buttercup.Web.Controllers.Queries;
using Buttercup.Web.Models.Recipes;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Buttercup.Web.Controllers;

public sealed class RecipesControllerTests : IDisposable
{
    private readonly ModelFactory modelFactory = new();

    private readonly FakeDbContextFactory dbContextFactory = new();
    private readonly DefaultHttpContext httpContext = new();
    private readonly DictionaryLocalizer<RecipesController> localizer = new();
    private readonly Mock<IRecipesControllerQueries> queriesMock = new();
    private readonly Mock<IRecipeManager> recipeManagerMock = new();

    private readonly RecipesController recipesController;

    public RecipesControllerTests() =>
        this.recipesController = new(
            this.dbContextFactory,
            this.localizer,
            this.queriesMock.Object,
            this.recipeManagerMock.Object)
        {
            ControllerContext = new() { HttpContext = this.httpContext },
        };

    public void Dispose() => this.recipesController.Dispose();

    #region Index

    [Fact]
    public async Task Index_ReturnsViewResultWithRecipes()
    {
        var recipes = new[] { this.modelFactory.BuildRecipe() };
        this.queriesMock
            .Setup(x => x.GetRecipesForIndex(this.dbContextFactory.FakeDbContext))
            .ReturnsAsync(recipes);

        var result = await this.recipesController.Index();
        var viewResult = Assert.IsType<ViewResult>(result);

        Assert.Same(recipes, viewResult.Model);
    }

    #endregion

    #region Show

    [Fact]
    public async Task Show_ReturnsViewResultWithRecipeAndComments()
    {
        var recipe = this.SetupFindRecipeForShowView();
        var comments = this.SetupGetCommentsForRecipe(recipe.Id);

        var result = await this.recipesController.Show(recipe.Id);
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ShowRecipeViewModel>(viewResult.Model);

        Assert.Equal(new(recipe, comments), model);
    }

    [Fact]
    public async Task Show_RecipeNotFoundOrAlreadySoftDeleted_ReturnsNotFoundResult()
    {
        var recipeId = this.modelFactory.NextInt();
        this.SetupFindRecipeForShowView(recipeId, null);

        var result = await this.recipesController.Show(recipeId);
        Assert.IsType<NotFoundResult>(result);
    }

    #endregion

    #region New (GET)

    [Fact]
    public void New_Get_ReturnsViewResult()
    {
        var result = this.recipesController.New();
        Assert.IsType<ViewResult>(result);
    }

    #endregion

    #region New (POST)

    [Fact]
    public async Task New_Post_Success_AddsRecipeAndRedirectsToShowPage()
    {
        var attributes = new RecipeAttributes(this.modelFactory.BuildRecipe());
        var currentUserId = this.SetupCurrentUserId();
        long recipeId = this.modelFactory.NextInt();

        this.recipeManagerMock
            .Setup(x => x.AddRecipe(attributes, currentUserId))
            .ReturnsAsync(recipeId);

        var result = await this.recipesController.New(attributes);

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(RecipesController.Show), redirectResult.ActionName);
        Assert.NotNull(redirectResult.RouteValues);
        Assert.Equal(recipeId, redirectResult.RouteValues["id"]);
    }

    [Fact]
    public async Task New_Post_InvalidModel_ReturnsViewResultWithEditModel()
    {
        var attributes = new RecipeAttributes(this.modelFactory.BuildRecipe());
        this.recipesController.ModelState.AddModelError("test", "test");

        var result = await this.recipesController.New(attributes);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(attributes, viewResult.Model);
    }

    #endregion

    #region Edit (GET)

    [Fact]
    public async Task Edit_Get_ReturnsViewResultWithEditModel()
    {
        var recipe = this.SetupFindRecipe();

        var result = await this.recipesController.Edit(recipe.Id);
        var viewResult = Assert.IsType<ViewResult>(result);

        var expectedModel = EditRecipeViewModel.ForRecipe(recipe);
        var actualModel = Assert.IsType<EditRecipeViewModel>(viewResult.Model);

        Assert.Equal(expectedModel, actualModel);
    }

    [Fact]
    public async Task Edit_Get_RecipeNotFoundOrAlreadySoftDeleted_ReturnsNotFoundResult()
    {
        var recipeId = this.modelFactory.NextInt();
        this.SetupFindRecipe(recipeId, null);

        var result = await this.recipesController.Edit(recipeId);
        Assert.IsType<NotFoundResult>(result);
    }

    #endregion

    #region Edit (POST)

    [Fact]
    public async Task Edit_Post_Success_UpdatesRecipeAndRedirectsToShowPage()
    {
        var editModel = EditRecipeViewModel.ForRecipe(this.modelFactory.BuildRecipe());
        var currentUserId = this.SetupCurrentUserId();

        var result = await this.recipesController.Edit(editModel.Id, editModel);

        this.recipeManagerMock.Verify(
            x => x.UpdateRecipe(
                editModel.Id, editModel.Attributes, editModel.BaseRevision, currentUserId));

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(RecipesController.Show), redirectResult.ActionName);
        Assert.NotNull(redirectResult.RouteValues);
        Assert.Equal(editModel.Id, redirectResult.RouteValues["id"]);
    }

    [Fact]
    public async Task Edit_Post_InvalidModel_ReturnsViewResultWithEditModel()
    {
        var editModel = EditRecipeViewModel.ForRecipe(this.modelFactory.BuildRecipe());
        this.recipesController.ModelState.AddModelError("test", "test");

        var result = await this.recipesController.Edit(editModel.Id, editModel);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(editModel, viewResult.Model);
    }

    [Fact]
    public async Task Edit_Post_ConcurrencyException_ReturnsViewResultAndAddsError()
    {
        var editModel = EditRecipeViewModel.ForRecipe(this.modelFactory.BuildRecipe());
        var currentUserId = this.SetupCurrentUserId();

        this.localizer.Add("Error_StaleEdit", "translated-stale-edit-error");

        this.recipeManagerMock
            .Setup(x => x.UpdateRecipe(
                editModel.Id, editModel.Attributes, editModel.BaseRevision, currentUserId))
            .ThrowsAsync(new ConcurrencyException());

        var result = await this.recipesController.Edit(editModel.Id, editModel);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(editModel, viewResult.Model);

        var formState = this.recipesController.ModelState[nameof(EditRecipeViewModel.Attributes)];
        Assert.NotNull(formState);

        var error = Assert.Single(formState.Errors);
        Assert.Equal("translated-stale-edit-error", error.ErrorMessage);
    }

    [Fact]
    public async Task Edit_Post_NotFoundException_ReturnsNotFoundResult()
    {
        var editModel = EditRecipeViewModel.ForRecipe(this.modelFactory.BuildRecipe());
        var currentUserId = this.SetupCurrentUserId();

        this.recipeManagerMock
            .Setup(x => x.UpdateRecipe(
                editModel.Id, editModel.Attributes, editModel.BaseRevision, currentUserId))
            .ThrowsAsync(new NotFoundException());

        var result = await this.recipesController.Edit(editModel.Id, editModel);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Post_SoftDeletedException_ReturnsNotFoundResult()
    {
        var editModel = EditRecipeViewModel.ForRecipe(this.modelFactory.BuildRecipe());
        var currentUserId = this.SetupCurrentUserId();

        this.recipeManagerMock
            .Setup(x => x.UpdateRecipe(
                editModel.Id, editModel.Attributes, editModel.BaseRevision, currentUserId))
            .ThrowsAsync(new SoftDeletedException());

        var result = await this.recipesController.Edit(editModel.Id, editModel);

        Assert.IsType<NotFoundResult>(result);
    }

    #endregion

    #region Delete (GET)

    [Fact]
    public async Task Delete_Get_ReturnsViewResultWithRecipe()
    {
        var recipe = this.SetupFindRecipe();

        var result = await this.recipesController.Delete(recipe.Id);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(recipe, viewResult.Model);
    }

    [Fact]
    public async Task Delete_Get_RecipeNotFoundOrAlreadySoftDeleted_ReturnsNotFoundResult()
    {
        var recipeId = this.modelFactory.NextInt();
        this.SetupFindRecipe(recipeId, null);

        var result = await this.recipesController.Delete(recipeId);
        Assert.IsType<NotFoundResult>(result);
    }

    #endregion

    #region Delete (POST)

    [Fact]
    public async Task Delete_Post_DeletesRecipeAndRedirectsToIndexPage()
    {
        var currentUserId = this.SetupCurrentUserId();
        var recipeId = this.modelFactory.NextInt();

        this.recipeManagerMock
            .Setup(x => x.DeleteRecipe(recipeId, currentUserId))
            .ReturnsAsync(true)
            .Verifiable();

        var result = await this.recipesController.DeletePost(recipeId);

        this.recipeManagerMock.Verify();

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(RecipesController.Index), redirectResult.ActionName);
    }

    [Fact]
    public async Task Delete_Post_RecipeNotFoundOrAlreadySoftDeleted_ReturnsNotFoundResult()
    {
        var currentUserId = this.SetupCurrentUserId();
        var recipeId = this.modelFactory.NextInt();

        this.recipeManagerMock
            .Setup(x => x.DeleteRecipe(recipeId, currentUserId))
            .ReturnsAsync(false);

        var result = await this.recipesController.DeletePost(recipeId);

        Assert.IsType<NotFoundResult>(result);
    }

    #endregion

    private long SetupCurrentUserId()
    {
        var userId = this.modelFactory.NextInt();
        this.httpContext.User = PrincipalFactory.CreateWithUserId(userId);
        return userId;
    }

    private Recipe SetupFindRecipe()
    {
        var recipe = this.modelFactory.BuildRecipe();
        this.SetupFindRecipe(recipe.Id, recipe);
        return recipe;
    }

    private void SetupFindRecipe(long id, Recipe? recipe) =>
        this.queriesMock
            .Setup(x => x.FindRecipe(this.dbContextFactory.FakeDbContext, id))
            .ReturnsAsync(recipe);

    private Recipe SetupFindRecipeForShowView()
    {
        var recipe = this.modelFactory.BuildRecipe();
        this.SetupFindRecipeForShowView(recipe.Id, recipe);
        return recipe;
    }

    private void SetupFindRecipeForShowView(long id, Recipe? recipe) =>
        this.queriesMock
            .Setup(x => x.FindRecipeForShowView(this.dbContextFactory.FakeDbContext, id))
            .ReturnsAsync(recipe);

    private Comment[] SetupGetCommentsForRecipe(long recipeId)
    {
        var comments = new[] { this.modelFactory.BuildComment() };
        this.queriesMock
            .Setup(x => x.GetCommentsForRecipe(this.dbContextFactory.FakeDbContext, recipeId))
            .ReturnsAsync(comments);
        return comments;
    }
}
