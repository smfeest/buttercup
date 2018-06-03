using System.Threading.Tasks;
using Buttercup.DataAccess;
using Microsoft.AspNetCore.Mvc;

namespace Buttercup.Web.Controllers
{
    [Route("recipes")]
    public class RecipesController : Controller
    {
        public RecipesController(
            IDbConnectionSource dbConnectionSource, IRecipeDataProvider recipeDataProvider)
        {
            this.DbConnectionSource = dbConnectionSource;
            this.RecipeDataProvider = recipeDataProvider;
        }

        public IDbConnectionSource DbConnectionSource { get; }

        public IRecipeDataProvider RecipeDataProvider { get; }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            using (var connection = await this.DbConnectionSource.OpenConnection())
            {
                return this.View(await this.RecipeDataProvider.GetRecipes(connection));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Show(long id)
        {
            using (var connection = await this.DbConnectionSource.OpenConnection())
            {
                return this.View(await this.RecipeDataProvider.GetRecipe(connection, id));
            }
        }
    }
}
