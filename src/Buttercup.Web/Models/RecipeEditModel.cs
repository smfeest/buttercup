using System.ComponentModel.DataAnnotations;
using Buttercup.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Buttercup.Web.Models;

public sealed record RecipeEditModel
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
    public long Id { get; init; }

    [Required(ErrorMessage = "Error_RequiredField")]
    [StringLength(255, ErrorMessage = "Error_TooManyCharacters")]
    public string Title { get; init; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "Error_OutOfRange")]
    public int? PreparationMinutes { get; init; }

    [Range(0, int.MaxValue, ErrorMessage = "Error_OutOfRange")]
    public int? CookingMinutes { get; init; }

    [Range(1, int.MaxValue, ErrorMessage = "Error_OutOfRange")]
    public int? Servings { get; init; }

    [Required(ErrorMessage = "Error_RequiredField")]
    [StringLength(32000, ErrorMessage = "Error_TooManyCharacters")]
    public string Ingredients { get; init; } = string.Empty;

    [Required(ErrorMessage = "Error_RequiredField")]
    [StringLength(32000, ErrorMessage = "Error_TooManyCharacters")]
    public string Method { get; init; } = string.Empty;

    [StringLength(32000, ErrorMessage = "Error_TooManyCharacters")]
    public string? Suggestions { get; init; }

    [StringLength(32000, ErrorMessage = "Error_TooManyCharacters")]
    public string? Remarks { get; init; }

    [StringLength(255, ErrorMessage = "Error_TooManyCharacters")]
    public string? Source { get; init; }

    public int Revision { get; init; }

    public Recipe ToRecipe() => new(
        this.Id,
        this.Title,
        this.PreparationMinutes,
        this.CookingMinutes,
        this.Servings,
        this.Ingredients,
        this.Method,
        this.Suggestions,
        this.Remarks,
        this.Source,
        new(),
        null,
        new(),
        null,
        this.Revision);
}
