using System.ComponentModel.DataAnnotations;
using Buttercup.Models;

namespace Buttercup.Web.Models
{
    public class RecipeEditModel
    {
        public RecipeEditModel()
        {
        }

        public RecipeEditModel(Recipe recipe)
        {
            this.Title = recipe.Title;
            this.PreparationMinutes = recipe.PreparationMinutes;
            this.CookingMinutes = recipe.CookingMinutes;
            this.Servings = recipe.Servings;
            this.Ingredients = recipe.Ingredients;
            this.Method = recipe.Method;
            this.Suggestions = recipe.Suggestions;
            this.Remarks = recipe.Remarks;
            this.Source = recipe.Source;
        }

        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        [Range(0, int.MaxValue)]
        public int? PreparationMinutes { get; set; }

        [Range(0, int.MaxValue)]
        public int? CookingMinutes { get; set; }

        [Range(1, int.MaxValue)]
        public int? Servings { get; set; }

        [Required]
        [StringLength(32000)]
        public string Ingredients { get; set; }

        [Required]
        [StringLength(32000)]
        public string Method { get; set; }

        [StringLength(32000)]
        public string Suggestions { get; set; }

        [StringLength(32000)]
        public string Remarks { get; set; }

        [StringLength(255)]
        public string Source { get; set; }

        public Recipe ToRecipe() => new Recipe
        {
            Title = this.Title,
            PreparationMinutes = this.PreparationMinutes,
            CookingMinutes = this.CookingMinutes,
            Servings = this.Servings,
            Ingredients = this.Ingredients,
            Method = this.Method,
            Suggestions = this.Suggestions,
            Remarks = this.Remarks,
            Source = this.Source,
        };
    }
}
