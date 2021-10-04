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
        private readonly IClock clock;
        private readonly IDbConnectionSource dbConnectionSource;
        private readonly IRecipeDataProvider recipeDataProvider;

        public RecipesController(
            IClock clock,
            IDbConnectionSource dbConnectionSource,
            IRecipeDataProvider recipeDataProvider)
        {
            this.clock = clock;
            this.dbConnectionSource = dbConnectionSource;
            this.recipeDataProvider = recipeDataProvider;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            using var connection = await this.dbConnectionSource.OpenConnection();

            return this.View(await this.recipeDataProvider.GetRecipes(connection));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Show(long id)
        {
            using var connection = await this.dbConnectionSource.OpenConnection();

            return this.View(await this.recipeDataProvider.GetRecipe(connection, id));
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

            recipe.Created = this.clock.UtcNow;
            recipe.CreatedByUserId = this.HttpContext.GetCurrentUser()!.Id;

            long id;

            using (var connection = await this.dbConnectionSource.OpenConnection())
            {
                id = await this.recipeDataProvider.AddRecipe(connection, recipe);
            }

            return this.RedirectToAction(nameof(this.Show), new { id = id });
        }

        [HttpGet("{id}/edit")]
        public async Task<IActionResult> Edit(long id)
        {
            using var connection = await this.dbConnectionSource.OpenConnection();

            return this.View(new RecipeEditModel(
                await this.recipeDataProvider.GetRecipe(connection, id)));
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
            recipe.Modified = this.clock.UtcNow;
            recipe.ModifiedByUserId = this.HttpContext.GetCurrentUser()!.Id;

            using (var connection = await this.dbConnectionSource.OpenConnection())
            {
                await this.recipeDataProvider.UpdateRecipe(connection, recipe);
            }

            return this.RedirectToAction(nameof(this.Show), new { id = id });
        }

        [HttpGet("{id}/delete")]
        public async Task<IActionResult> Delete(long id)
        {
            using var connection = await this.dbConnectionSource.OpenConnection();

            return this.View(await this.recipeDataProvider.GetRecipe(connection, id));
        }

        [HttpPost("{id}/delete")]
        public async Task<IActionResult> Delete(long id, int revision)
        {
            using var connection = await this.dbConnectionSource.OpenConnection();

            await this.recipeDataProvider.DeleteRecipe(connection, id, revision);

            return this.RedirectToAction(nameof(this.Index));
        }
    }
}
