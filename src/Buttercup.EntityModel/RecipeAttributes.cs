using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Buttercup.EntityModel;

/// <summary>
/// Represents a recipe's attributes.
/// </summary>
public record RecipeAttributes
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RecipeAttributes" /> class.
    /// </summary>
    public RecipeAttributes()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RecipeAttributes" /> class with values from
    /// another instance.
    /// </summary>
    /// <param name="source">
    /// The source object.
    /// </param>
    [SetsRequiredMembers]
    public RecipeAttributes(RecipeAttributes source)
    {
        this.Title = source.Title;
        this.PreparationMinutes = source.PreparationMinutes;
        this.CookingMinutes = source.CookingMinutes;
        this.Servings = source.Servings;
        this.Ingredients = source.Ingredients;
        this.Method = source.Method;
        this.Suggestions = source.Suggestions;
        this.Remarks = source.Remarks;
        this.Source = source.Source;
    }

    /// <summary>
    /// Gets or sets the title.
    /// </summary>
    /// <value>
    /// The title.
    /// </value>
    [Required(ErrorMessage = "Error_RequiredField")]
    [StringLength(250, ErrorMessage = "Error_TooManyCharacters")]
    public required string Title { get; set; }

    /// <summary>
    /// Gets or sets the preparation time in minutes.
    /// </summary>
    /// <value>
    /// The preparation time in minutes.
    /// </value>
    [Range(0, int.MaxValue, ErrorMessage = "Error_OutOfRange")]
    public int? PreparationMinutes { get; set; }

    /// <summary>
    /// Gets or sets the cooking time in minutes.
    /// </summary>
    /// <value>
    /// The cooking time in minutes.
    /// </value>
    [Range(0, int.MaxValue, ErrorMessage = "Error_OutOfRange")]
    public int? CookingMinutes { get; set; }

    /// <summary>
    /// Gets or sets the number of servings.
    /// </summary>
    /// <value>
    /// The number of servings.
    /// </value>
    [Range(1, int.MaxValue, ErrorMessage = "Error_OutOfRange")]
    public int? Servings { get; set; }

    /// <summary>
    /// Gets or sets the ingredients.
    /// </summary>
    /// <value>
    /// The ingredients.
    /// </value>
    [Column(TypeName = "text")]
    [Required(ErrorMessage = "Error_RequiredField")]
    [StringLength(32000, ErrorMessage = "Error_TooManyCharacters")]
    public required string Ingredients { get; set; }

    /// <summary>
    /// Gets or sets the method.
    /// </summary>
    /// <value>
    /// The method.
    /// </value>
    [Column(TypeName = "text")]
    [Required(ErrorMessage = "Error_RequiredField")]
    [StringLength(32000, ErrorMessage = "Error_TooManyCharacters")]
    public required string Method { get; set; }

    /// <summary>
    /// Gets or sets the suggestions.
    /// </summary>
    /// <value>
    /// The suggestions.
    /// </value>
    [Column(TypeName = "text")]
    [StringLength(32000, ErrorMessage = "Error_TooManyCharacters")]
    public string? Suggestions { get; set; }

    /// <summary>
    /// Gets or sets the remarks.
    /// </summary>
    /// <value>
    /// The remarks.
    /// </value>
    [Column(TypeName = "text")]
    [StringLength(32000, ErrorMessage = "Error_TooManyCharacters")]
    public string? Remarks { get; set; }

    /// <summary>
    /// Gets or sets the source.
    /// </summary>
    /// <value>
    /// The source.
    /// </value>
    [StringLength(250, ErrorMessage = "Error_TooManyCharacters")]
    public string? Source { get; set; }

    /// <summary>
    /// Copies all <see cref="RecipeAttributes"/> values over from another object.
    /// </summary>
    /// <param name="source">
    /// The source object.
    /// </param>
    public void CopyValuesFrom(RecipeAttributes source)
    {
        this.Title = source.Title;
        this.PreparationMinutes = source.PreparationMinutes;
        this.CookingMinutes = source.CookingMinutes;
        this.Servings = source.Servings;
        this.Ingredients = source.Ingredients;
        this.Method = source.Method;
        this.Suggestions = source.Suggestions;
        this.Remarks = source.Remarks;
        this.Source = source.Source;
    }
}
