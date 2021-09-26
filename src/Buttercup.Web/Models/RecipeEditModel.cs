using System.ComponentModel.DataAnnotations;
using Buttercup.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Buttercup.Web.Models
{
    public class RecipeEditModel
    {
        public RecipeEditModel()
        {
        }

        public RecipeEditModel(Recipe recipe)
        {
            this.Id = recipe.Id;
            this.Title = recipe.Title;
            this.PreparationMinutes = recipe.PreparationMinutes;
            this.CookingMinutes = recipe.CookingMinutes;
            this.Servings = recipe.Servings;
            this.Ingredients = recipe.Ingredients;
            this.Method = recipe.Method;
            this.Suggestions = recipe.Suggestions;
            this.Remarks = recipe.Remarks;
            this.Source = recipe.Source;
            this.Revision = recipe.Revision;
        }

        [BindNever]
        public long Id { get; set; }

        [Required(ErrorMessage = "Error_RequiredField")]
        [StringLength(255, ErrorMessage = "Error_TooManyCharacters")]
        public string Title { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Error_OutOfRange")]
        public int? PreparationMinutes { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Error_OutOfRange")]
        public int? CookingMinutes { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Error_OutOfRange")]
        public int? Servings { get; set; }

        [Required(ErrorMessage = "Error_RequiredField")]
        [StringLength(32000, ErrorMessage = "Error_TooManyCharacters")]
        public string Ingredients { get; set; }

        [Required(ErrorMessage = "Error_RequiredField")]
        [StringLength(32000, ErrorMessage = "Error_TooManyCharacters")]
        public string Method { get; set; }

        [StringLength(32000, ErrorMessage = "Error_TooManyCharacters")]
        public string Suggestions { get; set; }

        [StringLength(32000, ErrorMessage = "Error_TooManyCharacters")]
        public string Remarks { get; set; }

        [StringLength(255, ErrorMessage = "Error_TooManyCharacters")]
        public string Source { get; set; }

        public int Revision { get; set; }

        public Recipe ToRecipe() => new Recipe
        {
            Id = this.Id,
            Title = this.Title,
            PreparationMinutes = this.PreparationMinutes,
            CookingMinutes = this.CookingMinutes,
            Servings = this.Servings,
            Ingredients = this.Ingredients,
            Method = this.Method,
            Suggestions = this.Suggestions,
            Remarks = this.Remarks,
            Source = this.Source,
            Revision = this.Revision,
        };
    }
}
