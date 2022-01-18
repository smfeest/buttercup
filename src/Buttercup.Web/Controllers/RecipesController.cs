using Buttercup.DataAccess;
using Buttercup.Web.Authentication;
using Buttercup.Web.Filters;
using Buttercup.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Buttercup.Web.Controllers;

[Authorize]
[HandleNotFoundExceptionAttribute]
[Route("recipes")]
public class RecipesController : Controller
{
    private readonly IMySqlConnectionSource mySqlConnectionSource;
    private readonly IRecipeDataProvider recipeDataProvider;

    public RecipesController(
        IMySqlConnectionSource mySqlConnectionSource,
        IRecipeDataProvider recipeDataProvider)
    {
        this.mySqlConnectionSource = mySqlConnectionSource;
        this.recipeDataProvider = recipeDataProvider;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        using var connection = await this.mySqlConnectionSource.OpenConnection();

        return this.View(await this.recipeDataProvider.GetRecipes(connection));
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
    public async Task<IActionResult> New(RecipeEditModel model)
    {
        if (!this.ModelState.IsValid)
        {
            return this.View(model);
        }

        using var connection = await this.mySqlConnectionSource.OpenConnection();

        var id = await this.recipeDataProvider.AddRecipe(
            connection, model.Attributes, this.HttpContext.GetCurrentUser()!.Id);

        return this.RedirectToAction(nameof(this.Show), new { id });
    }

    [HttpGet("{id}/edit")]
    public async Task<IActionResult> Edit(long id)
    {
        using var connection = await this.mySqlConnectionSource.OpenConnection();

        return this.View(RecipeEditModel.ForRecipe(
            await this.recipeDataProvider.GetRecipe(connection, id)));
    }

    [HttpPost("{id}/edit")]
    public async Task<IActionResult> Edit(long id, RecipeEditModel model)
    {
        if (!this.ModelState.IsValid)
        {
            return this.View(model);
        }

        using var connection = await this.mySqlConnectionSource.OpenConnection();

        await this.recipeDataProvider.UpdateRecipe(
            connection,
            id,
            model.Attributes,
            model.Revision,
            this.HttpContext.GetCurrentUser()!.Id);

        return this.RedirectToAction(nameof(this.Show), new { id });
    }

    [HttpGet("{id}/delete")]
    public async Task<IActionResult> Delete(long id)
    {
        using var connection = await this.mySqlConnectionSource.OpenConnection();

        return this.View(await this.recipeDataProvider.GetRecipe(connection, id));
    }

    [HttpPost("{id}/delete")]
    public async Task<IActionResult> Delete(long id, int revision)
    {
        using var connection = await this.mySqlConnectionSource.OpenConnection();

        await this.recipeDataProvider.DeleteRecipe(connection, id, revision);

        return this.RedirectToAction(nameof(this.Index));
    }
}
