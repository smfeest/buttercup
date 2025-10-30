using Buttercup.Application;
using Buttercup.EntityModel;
using Buttercup.Security;
using Buttercup.Web.Controllers.Queries;
using Buttercup.Web.Models.Recipes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Buttercup.Web.Controllers;

[Authorize]
[Route("recipes")]
public sealed class RecipesController(
    ICommentManager commentManager,
    IDbContextFactory<AppDbContext> dbContextFactory,
    IStringLocalizer<RecipesController> localizer,
    IRecipesControllerQueries queries,
    IRecipeManager recipeManager)
    : Controller
{
    private readonly ICommentManager commentManager = commentManager;
    private readonly IDbContextFactory<AppDbContext> dbContextFactory = dbContextFactory;
    private readonly IStringLocalizer<RecipesController> localizer = localizer;
    private readonly IRecipesControllerQueries queries = queries;
    private readonly IRecipeManager recipeManager = recipeManager;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();
        return this.View(await this.queries.GetRecipesForIndex(dbContext));
    }

    [HttpGet("{id}")]
    public Task<IActionResult> Show(long id) => this.Show(id, new());

    [HttpGet("new")]
    public IActionResult New() => this.View();

    [HttpPost("new")]
    public async Task<IActionResult> New(RecipeAttributes model)
    {
        if (!this.ModelState.IsValid)
        {
            return this.View(model);
        }

        var id = await this.recipeManager.CreateRecipe(model, this.User.GetUserId());

        return this.RedirectToAction(nameof(this.Show), new { id });
    }

    [HttpGet("{id}/edit")]
    public async Task<IActionResult> Edit(long id)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();
        var recipe = await this.queries.FindRecipe(dbContext, id);
        return recipe is null ? this.NotFound() : this.View(EditRecipeViewModel.ForRecipe(recipe));
    }

    [HttpPost("{id}/edit")]
    public async Task<IActionResult> Edit(long id, EditRecipeViewModel model)
    {
        if (!this.ModelState.IsValid)
        {
            return this.View(model);
        }
        try
        {
            await this.recipeManager.UpdateRecipe(
                id, model.Attributes, model.BaseRevision, this.User.GetUserId());
        }
        catch (ConcurrencyException)
        {
            this.ModelState.AddModelError(
                nameof(EditRecipeViewModel.Attributes), this.localizer["Error_StaleEdit"]);

            return this.View(model);
        }
        catch (Exception e) when (e is NotFoundException or SoftDeletedException)
        {
            return this.NotFound();
        }

        return this.RedirectToAction(nameof(this.Show), new { id });
    }

    [HttpGet("{id}/delete")]
    public async Task<IActionResult> Delete(long id)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();
        var recipe = await this.queries.FindRecipe(dbContext, id);
        return recipe is null ? this.NotFound() : this.View(recipe);
    }

    [HttpPost("{id}/delete")]
    public async Task<IActionResult> DeletePost(long id) =>
        await this.recipeManager.DeleteRecipe(id, this.User.GetUserId()) ?
            this.RedirectToAction(nameof(this.Index)) :
            this.NotFound();

    [HttpPost("{id}/comments")]
    public async Task<IActionResult> AddComment(long id, CommentAttributes newCommentAttributes)
    {
        if (!this.ModelState.IsValid)
        {
            return await this.Show(id, newCommentAttributes);
        }

        long commentId;

        try
        {
            commentId = await this.commentManager.AddComment(
                id, newCommentAttributes, this.User.GetUserId());
        }
        catch (Exception e) when (e is NotFoundException or SoftDeletedException)
        {
            return this.NotFound();
        }

        return this.RedirectToAction(nameof(Show), null, new { id }, $"comment{commentId}");
    }

    private async Task<IActionResult> Show(long id, CommentAttributes newCommentAttributes)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        var recipe = await this.queries.FindRecipeForShowView(dbContext, id);

        if (recipe is null)
        {
            return this.NotFound();
        }

        var comments = await this.queries.GetCommentsForRecipe(dbContext, id);

        return this.View(
            nameof(Show),
            new ShowRecipeViewModel(recipe, comments, newCommentAttributes, this.User));
    }
}
