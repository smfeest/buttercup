using Buttercup.DataAccess;
using Buttercup.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Buttercup.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly IMySqlConnectionSource mySqlConnectionSource;
    private readonly IRecipeDataProvider recipeDataProvider;

    public HomeController(
        IMySqlConnectionSource mySqlConnectionSource, IRecipeDataProvider recipeDataProvider)
    {
        this.mySqlConnectionSource = mySqlConnectionSource;
        this.recipeDataProvider = recipeDataProvider;
    }

    [HttpGet("/")]
    public async Task<IActionResult> Index()
    {
        var connection = await this.mySqlConnectionSource.OpenConnection();

        var recentlyAdded = await this.recipeDataProvider.GetRecentlyAddedRecipes(connection);
        var recentlyUpdated = await this.recipeDataProvider.GetRecentlyUpdatedRecipes(
            connection, recentlyAdded.Select(r => r.Id).ToArray());

        return this.View(new HomePageViewModel(recentlyAdded, recentlyUpdated));
    }
}
