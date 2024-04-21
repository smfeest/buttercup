using Buttercup.Application;
using Buttercup.EntityModel;
using Buttercup.Security;
using Buttercup.Web.Controllers.Queries;
using Buttercup.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Buttercup.Web.Controllers;

[Authorize]
[Route("recipes")]
public sealed class RecipesController(
    IStringLocalizer<RecipesController> localizer,
    IRecipesControllerQueries queries,
    IRecipeManager recipeManager)
    : Controller
{
    private readonly IStringLocalizer<RecipesController> localizer = localizer;
    private readonly IRecipesControllerQueries queries = queries;
    private readonly IRecipeManager recipeManager = recipeManager;

    [HttpGet]
    public async Task<IActionResult> Index() =>
        this.View(await this.queries.GetRecipes());

    [HttpGet("{id}")]
    public async Task<IActionResult> Show(long id)
    {
        var recipe = await this.queries.FindRecipe(
            id, includeCreatedAndModifiedByUser: true);
        return recipe is null ? this.NotFound() : this.View(recipe);
    }

    [HttpGet("new")]
    public IActionResult New() => this.View();

    [HttpPost("new")]
    public async Task<IActionResult> New(RecipeAttributes model)
    {
        if (!this.ModelState.IsValid)
        {
            return this.View(model);
        }

        var id = await this.recipeManager.AddRecipe(model, this.User.GetUserId());

        return this.RedirectToAction(nameof(this.Show), new { id });
    }

    [HttpGet("{id}/edit")]
    public async Task<IActionResult> Edit(long id)
    {
        var recipe = await this.queries.FindRecipe(id);
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
        catch (NotFoundException)
        {
            return this.NotFound();
        }
        catch (SoftDeletedException)
        {
            return this.NotFound();
        }

        return this.RedirectToAction(nameof(this.Show), new { id });
    }

    [HttpGet("{id}/delete")]
    public async Task<IActionResult> Delete(long id)
    {
        var recipe = await this.queries.FindRecipe(id);
        return recipe is null ? this.NotFound() : this.View(recipe);
    }

    [HttpPost("{id}/delete")]
    public async Task<IActionResult> DeletePost(long id) =>
        await this.recipeManager.DeleteRecipe(id, this.User.GetUserId()) ?
            this.RedirectToAction(nameof(this.Index)) :
            this.NotFound();
}
