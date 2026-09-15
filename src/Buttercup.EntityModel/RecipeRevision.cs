using System.Diagnostics.CodeAnalysis;

namespace Buttercup.EntityModel;

/// <summary>
/// Represents a reversion of a recipe.
/// </summary>
public sealed record RecipeRevision : RecipeAttributes
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RecipeRevision" /> class.
    /// </summary>
    public RecipeRevision()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RecipeRevision" /> class.
    /// </summary>
    /// <param name="recipe">
    /// The recipe.
    /// </param>
    [SetsRequiredMembers]
    public RecipeRevision(Recipe recipe) : base(recipe)
    {
        this.Recipe = recipe;
        this.RecipeId = recipe.Id;
    }

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
}
