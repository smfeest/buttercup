using Buttercup.Web.Controllers.Queries;
using Buttercup.Web.Models.Home;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Buttercup.Web.Controllers;

[Authorize]
public sealed class HomeController(IHomeControllerQueries queries) : Controller
{
    private readonly IHomeControllerQueries queries = queries;

    [HttpGet("/")]
    public async Task<IActionResult> Index()
    {
        var recentlyAdded = await this.queries.GetRecentlyAddedRecipes();
        var recentlyUpdated = await this.queries.GetRecentlyUpdatedRecipes(
            recentlyAdded.Select(r => r.Id).ToArray());

        return this.View(new HomePageViewModel(recentlyAdded, recentlyUpdated));
    }
}
