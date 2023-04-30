namespace Buttercup.EntityModel;

/// <summary>
/// Represents a recipe.
/// </summary>
public sealed record Recipe
{
    /// <summary>
    /// Gets or sets the primary key of the recipe.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Gets or sets the recipe title.
    /// </summary>
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
    public string? Source { get; set; }

    /// <summary>
    /// Gets or sets the date and time at which the recipe was added.
    /// </summary>
    public required DateTime Created { get; set; }

    /// <summary>
    /// Gets or sets the user who added the recipe.
    /// </summary>
    public User? CreatedByUser { get; set; }

    /// <summary>
    /// Gets or sets the primary key of the user who added the recipe.
    /// </summary>
    public long? CreatedByUserId { get; set; }

    /// <summary>
    /// Gets or sets the date and time at which the recipe was last modified.
    /// </summary>
    public required DateTime Modified { get; set; }

    /// <summary>
    /// Gets or sets the user who last modified the recipe.
    /// </summary>
    public User? ModifiedByUser { get; set; }

    /// <summary>
    /// Gets or sets the primary key of the user who last modified the recipe.
    /// </summary>
    public long? ModifiedByUserId { get; set; }

    /// <summary>
    /// Gets or sets the revision number for concurrency control.
    /// </summary>
    public int Revision { get; set; }
}
