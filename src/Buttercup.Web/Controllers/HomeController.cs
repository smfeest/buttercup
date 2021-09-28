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
        public HomeController(
            IDbConnectionSource dbConnectionSource, IRecipeDataProvider recipeDataProvider)
        {
            this.DbConnectionSource = dbConnectionSource;
            this.RecipeDataProvider = recipeDataProvider;
        }

        public IDbConnectionSource DbConnectionSource { get; }

        public IRecipeDataProvider RecipeDataProvider { get; }

        [HttpGet("/")]
        public async Task<IActionResult> Index()
        {
            var connection = await this.DbConnectionSource.OpenConnection();

            return this.View(new HomePageViewModel
            {
                RecentlyAddedRecipes =
                    await this.RecipeDataProvider.GetRecentlyAddedRecipes(connection),
                RecentlyUpdatedRecipes =
                    await this.RecipeDataProvider.GetRecentlyUpdatedRecipes(connection),
            });
        }
    }
}
