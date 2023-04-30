using Buttercup.DataAccess;
using Buttercup.EntityModel;
using Buttercup.Web.Authentication;
using Buttercup.Web.Filters;
using Buttercup.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Buttercup.Web.Controllers;

[Authorize]
[HandleNotFoundExceptionAttribute]
[Route("recipes")]
public class RecipesController : Controller
{
    private readonly IStringLocalizer<RecipesController> localizer;
    private readonly IMySqlConnectionSource mySqlConnectionSource;
    private readonly IRecipeDataProvider recipeDataProvider;

    public RecipesController(
        IStringLocalizer<RecipesController> localizer,
        IMySqlConnectionSource mySqlConnectionSource,
        IRecipeDataProvider recipeDataProvider)
    {
        this.localizer = localizer;
        this.mySqlConnectionSource = mySqlConnectionSource;
        this.recipeDataProvider = recipeDataProvider;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        using var connection = await this.mySqlConnectionSource.OpenConnection();

        return this.View(await this.recipeDataProvider.GetAllRecipes(connection));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Show(long id)
    {
        using var connection = await this.mySqlConnectionSource.OpenConnection();

        return this.View(await this.recipeDataProvider.GetRecipe(connection, id));
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

        using var connection = await this.mySqlConnectionSource.OpenConnection();

        var id = await this.recipeDataProvider.AddRecipe(
            connection, model, this.HttpContext.GetCurrentUser()!.Id);

        return this.RedirectToAction(nameof(this.Show), new { id });
    }

    [HttpGet("{id}/edit")]
    public async Task<IActionResult> Edit(long id)
    {
        using var connection = await this.mySqlConnectionSource.OpenConnection();

        return this.View(EditRecipeViewModel.ForRecipe(
            await this.recipeDataProvider.GetRecipe(connection, id)));
    }

    [HttpPost("{id}/edit")]
    public async Task<IActionResult> Edit(long id, EditRecipeViewModel model)
    {
        if (!this.ModelState.IsValid)
        {
            return this.View(model);
        }

        using var connection = await this.mySqlConnectionSource.OpenConnection();

        try
        {
            await this.recipeDataProvider.UpdateRecipe(
                connection,
                id,
                model.Attributes,
                model.BaseRevision,
                this.HttpContext.GetCurrentUser()!.Id);
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
    public async Task<IActionResult> Delete(long id)
    {
        using var connection = await this.mySqlConnectionSource.OpenConnection();

        return this.View(await this.recipeDataProvider.GetRecipe(connection, id));
    }

    [HttpPost("{id}/delete")]
    public async Task<IActionResult> DeletePost(long id)
    {
        using var connection = await this.mySqlConnectionSource.OpenConnection();

        await this.recipeDataProvider.DeleteRecipe(connection, id);

        return this.RedirectToAction(nameof(this.Index));
    }
}
