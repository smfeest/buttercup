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

        var recipe = model.ToRecipe() with
        {
            CreatedByUserId = this.HttpContext.GetCurrentUser()!.Id,
        };

        long id;

        using (var connection = await this.mySqlConnectionSource.OpenConnection())
        {
            id = await this.recipeDataProvider.AddRecipe(connection, recipe);
        }

        return this.RedirectToAction(nameof(this.Show), new { id });
    }

    [HttpGet("{id}/edit")]
    public async Task<IActionResult> Edit(long id)
    {
        using var connection = await this.mySqlConnectionSource.OpenConnection();

        return this.View(new RecipeEditModel(
            await this.recipeDataProvider.GetRecipe(connection, id)));
    }

    [HttpPost("{id}/edit")]
    public async Task<IActionResult> Edit(long id, RecipeEditModel model)
    {
        if (!this.ModelState.IsValid)
        {
            return this.View(model);
        }

        var recipe = model.ToRecipe() with
        {
            Id = id,
            ModifiedByUserId = this.HttpContext.GetCurrentUser()!.Id,
        };

        using (var connection = await this.mySqlConnectionSource.OpenConnection())
        {
            await this.recipeDataProvider.UpdateRecipe(connection, recipe);
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
    public async Task<IActionResult> Delete(long id, int revision)
    {
        using var connection = await this.mySqlConnectionSource.OpenConnection();

        await this.recipeDataProvider.DeleteRecipe(connection, id, revision);

        return this.RedirectToAction(nameof(this.Index));
    }
}
