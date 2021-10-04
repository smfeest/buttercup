using System.Threading.Tasks;
using Buttercup.DataAccess;
using Buttercup.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Buttercup.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IDbConnectionSource dbConnectionSource;
        private readonly IRecipeDataProvider recipeDataProvider;

        public HomeController(
            IDbConnectionSource dbConnectionSource, IRecipeDataProvider recipeDataProvider)
        {
            this.dbConnectionSource = dbConnectionSource;
            this.recipeDataProvider = recipeDataProvider;
        }

        [HttpGet("/")]
        public async Task<IActionResult> Index()
        {
            var connection = await this.dbConnectionSource.OpenConnection();

            return this.View(new HomePageViewModel
            {
                RecentlyAddedRecipes =
                    await this.recipeDataProvider.GetRecentlyAddedRecipes(connection),
                RecentlyUpdatedRecipes =
                    await this.recipeDataProvider.GetRecentlyUpdatedRecipes(connection),
            });
        }
    }
}
