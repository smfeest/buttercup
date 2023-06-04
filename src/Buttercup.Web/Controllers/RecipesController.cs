using Buttercup.DataAccess;
using Buttercup.EntityModel;
using Buttercup.Web.Authentication;
using Buttercup.Web.Filters;
using Buttercup.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Buttercup.Web.Controllers;

[Authorize]
[HandleNotFoundExceptionAttribute]
[Route("recipes")]
public sealed class RecipesController : Controller
{
    private readonly IDbContextFactory<AppDbContext> dbContextFactory;
    private readonly IStringLocalizer<RecipesController> localizer;
    private readonly IRecipeDataProvider recipeDataProvider;

    public RecipesController(
        IDbContextFactory<AppDbContext> dbContextFactory,
        IStringLocalizer<RecipesController> localizer,
        IRecipeDataProvider recipeDataProvider)
    {
        this.dbContextFactory = dbContextFactory;
        this.localizer = localizer;
        this.recipeDataProvider = recipeDataProvider;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        return this.View(await this.recipeDataProvider.GetAllRecipes(dbContext));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Show(long id)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        return this.View(await this.recipeDataProvider.GetRecipe(dbContext, id));
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

        using var dbContext = this.dbContextFactory.CreateDbContext();

        var id = await this.recipeDataProvider.AddRecipe(
            dbContext, model, this.HttpContext.GetCurrentUser()!.Id);

        return this.RedirectToAction(nameof(this.Show), new { id });
    }

    [HttpGet("{id}/edit")]
    public async Task<IActionResult> Edit(long id)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        return this.View(EditRecipeViewModel.ForRecipe(
            await this.recipeDataProvider.GetRecipe(dbContext, id)));
    }

    [HttpPost("{id}/edit")]
    public async Task<IActionResult> Edit(long id, EditRecipeViewModel model)
    {
        if (!this.ModelState.IsValid)
        {
            return this.View(model);
        }

        using var dbContext = this.dbContextFactory.CreateDbContext();

        try
        {
            await this.recipeDataProvider.UpdateRecipe(
                dbContext,
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
        using var dbContext = this.dbContextFactory.CreateDbContext();

        return this.View(await this.recipeDataProvider.GetRecipe(dbContext, id));
    }

    [HttpPost("{id}/delete")]
    public async Task<IActionResult> DeletePost(long id)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        await this.recipeDataProvider.DeleteRecipe(dbContext, id);

        return this.RedirectToAction(nameof(this.Index));
    }
}
