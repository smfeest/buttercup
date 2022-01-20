using System.ComponentModel.DataAnnotations;

namespace Buttercup.Models;

/// <summary>
/// Represents a recipe's attributes.
/// </summary>
public sealed record RecipeAttributes
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RecipeAttributes" /> class.
    /// </summary>
    public RecipeAttributes()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RecipeAttributes" /> class with the attribute
    /// values from a recipe.
    /// </summary>
    /// <param name="recipe">
    /// The recipe.
    /// </param>
    public RecipeAttributes(Recipe recipe)
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

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    /// <value>
    /// The title.
    /// </value>
    [Required(ErrorMessage = "Error_RequiredField")]
    [StringLength(255, ErrorMessage = "Error_TooManyCharacters")]
    public string Title { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the preparation time in minutes.
    /// </summary>
    /// <value>
    /// The preparation time in minutes.
    /// </value>
    [Range(0, int.MaxValue, ErrorMessage = "Error_OutOfRange")]
    public int? PreparationMinutes { get; init; }

    /// <summary>
    /// Gets or sets the cooking time in minutes.
    /// </summary>
    /// <value>
    /// The cooking time in minutes.
    /// </value>
    [Range(0, int.MaxValue, ErrorMessage = "Error_OutOfRange")]
    public int? CookingMinutes { get; init; }

    /// <summary>
    /// Gets or sets the number of servings.
    /// </summary>
    /// <value>
    /// The number of servings.
    /// </value>
    [Range(1, int.MaxValue, ErrorMessage = "Error_OutOfRange")]
    public int? Servings { get; init; }

    /// <summary>
    /// Gets or sets the ingredients.
    /// </summary>
    /// <value>
    /// The ingredients.
    /// </value>
    [Required(ErrorMessage = "Error_RequiredField")]
    [StringLength(32000, ErrorMessage = "Error_TooManyCharacters")]
    public string Ingredients { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the method.
    /// </summary>
    /// <value>
    /// The method.
    /// </value>
    [Required(ErrorMessage = "Error_RequiredField")]
    [StringLength(32000, ErrorMessage = "Error_TooManyCharacters")]
    public string Method { get; init; } = string.Empty;

    /// <summary>
    /// Gets or sets the suggestions.
    /// </summary>
    /// <value>
    /// The suggestions.
    /// </value>
    [StringLength(32000, ErrorMessage = "Error_TooManyCharacters")]
    public string? Suggestions { get; init; }

    /// <summary>
    /// Gets or sets the remarks.
    /// </summary>
    /// <value>
    /// The remarks.
    /// </value>
    [StringLength(32000, ErrorMessage = "Error_TooManyCharacters")]
    public string? Remarks { get; init; }

    /// <summary>
    /// Gets or sets the source.
    /// </summary>
    /// <value>
    /// The source.
    /// </value>
    [StringLength(255, ErrorMessage = "Error_TooManyCharacters")]
    public string? Source { get; init; }
}
