using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Buttercup.DataAccess;
using Buttercup.Web.Models;
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

        [HttpGet("new")]
        public IActionResult New() => this.View();

        [HttpPost("new")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> New(RecipeEditModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            long id;

            using (var connection = await this.DbConnectionSource.OpenConnection())
            {
                id = await this.RecipeDataProvider.AddRecipe(connection, model.ToRecipe());
            }

            return this.RedirectToAction(nameof(this.Show), new { id = id });
        }

        [HttpGet("{id}/edit")]
        public async Task<IActionResult> Edit(long id)
        {
            using (var connection = await this.DbConnectionSource.OpenConnection())
            {
                return this.View(new RecipeEditModel(
                    await this.RecipeDataProvider.GetRecipe(connection, id)));
            }
        }

        [HttpPost("{id}/edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, RecipeEditModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            var recipe = model.ToRecipe();
            recipe.Id = id;

            using (var connection = await this.DbConnectionSource.OpenConnection())
            {
                await this.RecipeDataProvider.UpdateRecipe(connection, recipe);
            }

            return this.RedirectToAction(nameof(this.Show), new { id = id });
        }
    }
}
