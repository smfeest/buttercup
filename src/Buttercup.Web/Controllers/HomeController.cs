using Buttercup.DataAccess;
using Buttercup.EntityModel;
using Buttercup.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Buttercup.Web.Controllers;

[Authorize]
public sealed class HomeController : Controller
{
    private readonly IDbContextFactory<AppDbContext> dbContextFactory;
    private readonly IRecipeDataProvider recipeDataProvider;

    public HomeController(
        IDbContextFactory<AppDbContext> dbContextFactory, IRecipeDataProvider recipeDataProvider)
    {
        this.dbContextFactory = dbContextFactory;
        this.recipeDataProvider = recipeDataProvider;
    }

    [HttpGet("/")]
    public async Task<IActionResult> Index()
    {
        var dbContext = this.dbContextFactory.CreateDbContext();

        var recentlyAdded = await this.recipeDataProvider.GetRecentlyAddedRecipes(dbContext);
        var recentlyUpdated = await this.recipeDataProvider.GetRecentlyUpdatedRecipes(
            dbContext, recentlyAdded.Select(r => r.Id).ToArray());

        return this.View(new HomePageViewModel(recentlyAdded, recentlyUpdated));
    }
}
