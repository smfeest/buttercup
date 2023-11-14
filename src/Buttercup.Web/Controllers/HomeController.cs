using Buttercup.Application;
using Buttercup.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Buttercup.Web.Controllers;

[Authorize]
public sealed class HomeController : Controller
{
    private readonly IRecipeManager RecipeManager;

    public HomeController(IRecipeManager RecipeManager) => this.RecipeManager = RecipeManager;

    [HttpGet("/")]
    public async Task<IActionResult> Index()
    {
        var recentlyAdded = await this.RecipeManager.GetRecentlyAddedRecipes();
        var recentlyUpdated = await this.RecipeManager.GetRecentlyUpdatedRecipes(
            recentlyAdded.Select(r => r.Id).ToArray());

        return this.View(new HomePageViewModel(recentlyAdded, recentlyUpdated));
    }
}
