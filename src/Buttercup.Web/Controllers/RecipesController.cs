using Buttercup.Application;
using Buttercup.Security;
using Buttercup.Web.Filters;
using Buttercup.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Buttercup.Web.Controllers;

[Authorize]
[HandleNotFoundException]
[Route("recipes")]
public sealed class RecipesController(
    IStringLocalizer<RecipesController> localizer, IRecipeManager RecipeManager)
    : Controller
{
    private readonly IStringLocalizer<RecipesController> localizer = localizer;
    private readonly IRecipeManager RecipeManager = RecipeManager;

    [HttpGet]
    public async Task<IActionResult> Index() => this.View(await this.RecipeManager.GetAllRecipes());

    [HttpGet("{id}")]
    public async Task<IActionResult> Show(long id) =>
        this.View(await this.RecipeManager.GetRecipe(id));

    [HttpGet("new")]
    public IActionResult New() => this.View();

    [HttpPost("new")]
    public async Task<IActionResult> New(RecipeAttributes model)
    {
        if (!this.ModelState.IsValid)
        {
            return this.View(model);
        }

        var id = await this.RecipeManager.AddRecipe(model, this.User.GetUserId());

        return this.RedirectToAction(nameof(this.Show), new { id });
    }

    [HttpGet("{id}/edit")]
    public async Task<IActionResult> Edit(long id) =>
        this.View(EditRecipeViewModel.ForRecipe(await this.RecipeManager.GetRecipe(id)));

    [HttpPost("{id}/edit")]
    public async Task<IActionResult> Edit(long id, EditRecipeViewModel model)
    {
        if (!this.ModelState.IsValid)
        {
            return this.View(model);
        }
        try
        {
            await this.RecipeManager.UpdateRecipe(
                id, model.Attributes, model.BaseRevision, this.User.GetUserId());
        }
        catch (ConcurrencyException)
        {
            this.ModelState.AddModelError(
                nameof(EditRecipeViewModel.Attributes), this.localizer["Error_StaleEdit"]);

            return this.View(model);
        }

        return this.RedirectToAction(nameof(this.Show), new { id });
    }

    [HttpGet("{id}/delete")]
    public async Task<IActionResult> Delete(long id) =>
        this.View(await this.RecipeManager.GetRecipe(id));

    [HttpPost("{id}/delete")]
    public async Task<IActionResult> DeletePost(long id)
    {
        await this.RecipeManager.DeleteRecipe(id);

        return this.RedirectToAction(nameof(this.Index));
    }
}
