using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Buttercup.EntityModel;

/// <summary>
/// Represents a reversion of a recipe.
/// </summary>
[PrimaryKey(nameof(RecipeId), nameof(Revision))]
public sealed record RecipeRevision
{
    /// <summary>
    /// Gets or sets the recipe.
    /// </summary>
    public Recipe? Recipe { get; set; }

    /// <summary>
    /// Gets or sets the primary key of the recipe.
    /// </summary>
    public long RecipeId { get; set; }

    /// <summary>
    /// Gets or sets the revision number.
    /// </summary>
    public int Revision { get; set; }

    /// <summary>
    /// Gets or sets the date and time at which the revision was added.
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// Gets or sets the user who created the revision.
    /// </summary>
    public User? CreatedByUser { get; set; }

    /// <summary>
    /// Gets or sets the primary key of the user who created the revision.
    /// </summary>
    public long? CreatedByUserId { get; set; }

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
    [Column(TypeName = "text")]
    public required string Ingredients { get; set; }

    /// <summary>
    /// Gets or sets the method.
    /// </summary>
    [Column(TypeName = "text")]
    public required string Method { get; set; }

    /// <summary>
    /// Gets or sets the suggestions for the recipe.
    /// </summary>
    [Column(TypeName = "text")]
    public string? Suggestions { get; set; }

    /// <summary>
    /// Gets or sets the remarks for the recipe.
    /// </summary>
    [Column(TypeName = "text")]
    public string? Remarks { get; set; }

    /// <summary>
    /// Gets or sets the source of the recipe.
    /// </summary>
    [StringLength(250)]
    public string? Source { get; set; }

    /// <summary>
    /// Creates a new <see cref="RecipeRevision" /> copying property values from a <see
    /// cref="Recipe" />.
    /// </summary>
    /// <param name="recipe">
    /// The recipe to copy property values from.
    /// </param>
    /// <returns>
    /// The new <see cref="RecipeRevision"/>.
    /// </returns>
    public static RecipeRevision From(Recipe recipe) => new()
    {
        Recipe = recipe,
        RecipeId = recipe.Id,
        Revision = recipe.Revision,
        Created = recipe.Modified,
        CreatedByUser = recipe.ModifiedByUser,
        CreatedByUserId = recipe.ModifiedByUserId,
        Title = recipe.Title,
        PreparationMinutes = recipe.PreparationMinutes,
        CookingMinutes = recipe.CookingMinutes,
        Servings = recipe.Servings,
        Ingredients = recipe.Ingredients,
        Method = recipe.Method,
        Suggestions = recipe.Suggestions,
        Remarks = recipe.Remarks,
        Source = recipe.Source,
    };
}
