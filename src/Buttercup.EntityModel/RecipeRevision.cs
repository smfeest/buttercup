using System.ComponentModel.DataAnnotations;

namespace Buttercup.EntityModel;

/// <summary>
/// Represents a reversion of a recipe.
/// </summary>
public sealed record RecipeRevision
{
    /// <summary>
    /// Gets or sets the primary key of the revision.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the recipe.
    /// </summary>
    public Recipe? Recipe { get; set; }

    /// <summary>
    /// Gets or sets the primary key of the recipe.
    /// </summary>
    public long RecipeId { get; set; }

    /// <summary>
    /// Gets or sets the recipe title.
    /// </summary>
    [StringLength(250)]
    public required string Title { get; set; }

    /// <summary>
    /// Gets or sets the preparation time in minutes.
    /// </summary>
    public int? PreparationMinutes { get; set; }

    /// <summary>
    /// Gets or sets the cooking time in minutes.
    /// </summary>
    public int? CookingMinutes { get; set; }

    /// <summary>
    /// Gets or sets the number of servings.
    /// </summary>
    public int? Servings { get; set; }

    /// <summary>
    /// Gets or sets the ingredients.
    /// </summary>
    public required string Ingredients { get; set; }

    /// <summary>
    /// Gets or sets the method.
    /// </summary>
    public required string Method { get; set; }

    /// <summary>
    /// Gets or sets the suggestions for the recipe.
    /// </summary>
    public string? Suggestions { get; set; }

    /// <summary>
    /// Gets or sets the remarks for the recipe.
    /// </summary>
    public string? Remarks { get; set; }

    /// <summary>
    /// Gets or sets the source of the recipe.
    /// </summary>
    [StringLength(250)]
    public string? Source { get; set; }
}
