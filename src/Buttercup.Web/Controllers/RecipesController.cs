using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Buttercup.DataAccess;
using Buttercup.Web.Authentication;
using Buttercup.Web.Filters;
using Buttercup.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Buttercup.Web.Controllers
{
    [Authorize]
    [HandleNotFoundExceptionAttribute]
    [Route("recipes")]
    public class RecipesController : Controller
    {
        public RecipesController(
            IClock clock,
            IDbConnectionSource dbConnectionSource,
            IRecipeDataProvider recipeDataProvider)
        {
            this.Clock = clock;
            this.DbConnectionSource = dbConnectionSource;
            this.RecipeDataProvider = recipeDataProvider;
        }

        public IClock Clock { get; }

        public IDbConnectionSource DbConnectionSource { get; }

        public IRecipeDataProvider RecipeDataProvider { get; }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            using var connection = await this.DbConnectionSource.OpenConnection();

            return this.View(await this.RecipeDataProvider.GetRecipes(connection));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Show(long id)
        {
            using var connection = await this.DbConnectionSource.OpenConnection();

            return this.View(await this.RecipeDataProvider.GetRecipe(connection, id));
        }

        [HttpGet("new")]
        public IActionResult New() => this.View();

        [HttpPost("new")]
        public async Task<IActionResult> New(RecipeEditModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            var recipe = model.ToRecipe();

            recipe.Created = this.Clock.UtcNow;
            recipe.CreatedByUserId = this.HttpContext.GetCurrentUser().Id;

            long id;

            using (var connection = await this.DbConnectionSource.OpenConnection())
            {
                id = await this.RecipeDataProvider.AddRecipe(connection, recipe);
            }

            return this.RedirectToAction(nameof(this.Show), new { id = id });
        }

        [HttpGet("{id}/edit")]
        public async Task<IActionResult> Edit(long id)
        {
            using var connection = await this.DbConnectionSource.OpenConnection();

            return this.View(new RecipeEditModel(
                await this.RecipeDataProvider.GetRecipe(connection, id)));
        }

        [HttpPost("{id}/edit")]
        public async Task<IActionResult> Edit(long id, RecipeEditModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            var recipe = model.ToRecipe();

            recipe.Id = id;
            recipe.Modified = this.Clock.UtcNow;
            recipe.ModifiedByUserId = this.HttpContext.GetCurrentUser().Id;

            using (var connection = await this.DbConnectionSource.OpenConnection())
            {
                await this.RecipeDataProvider.UpdateRecipe(connection, recipe);
            }

            return this.RedirectToAction(nameof(this.Show), new { id = id });
        }

        [HttpGet("{id}/delete")]
        public async Task<IActionResult> Delete(long id)
        {
            using var connection = await this.DbConnectionSource.OpenConnection();

            return this.View(await this.RecipeDataProvider.GetRecipe(connection, id));
        }

        [HttpPost("{id}/delete")]
        public async Task<IActionResult> Delete(long id, int revision)
        {
            using var connection = await this.DbConnectionSource.OpenConnection();

            await this.RecipeDataProvider.DeleteRecipe(connection, id, revision);

            return this.RedirectToAction(nameof(this.Index));
        }
    }
}
