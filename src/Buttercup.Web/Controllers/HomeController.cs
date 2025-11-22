using Buttercup.EntityModel;
using Buttercup.Web.Controllers.Queries;
using Buttercup.Web.Models.Home;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Buttercup.Web.Controllers;

[Authorize]
public sealed class HomeController(
    IDbContextFactory<AppDbContext> dbContextFactory, IHomeControllerQueries queries) : Controller
{
    private readonly IDbContextFactory<AppDbContext> dbContextFactory = dbContextFactory;
    private readonly IHomeControllerQueries queries = queries;

    [HttpGet("/")]
    public async Task<IActionResult> Index()
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        var recentlyAdded = await this.queries.GetRecentlyAddedRecipes(dbContext);
        var recentlyUpdated = await this.queries.GetRecentlyUpdatedRecipes(
            dbContext, [.. recentlyAdded.Select(r => r.Id)]);

        return this.View(new HomePageViewModel(recentlyAdded, recentlyUpdated));
    }
}
