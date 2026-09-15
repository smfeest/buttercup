using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Buttercup.EntityModel;

/// <summary>
/// Represents a recipe.
/// </summary>
[Index(nameof(Title))]
[Index(nameof(Created))]
[Index(nameof(Modified))]
[Index(nameof(Deleted))]
public sealed record Recipe : RecipeAttributes, IEntityId, ISoftDeletable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Recipe" /> class.
    /// </summary>
    public Recipe()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Recipe" /> class.
    /// </summary>
    /// <param name="attributes">
    /// The recipe attributes.
    /// </param>
    [SetsRequiredMembers]
    public Recipe(RecipeAttributes attributes) : base(attributes)
    { }

    /// <summary>
    /// Gets or sets the primary key of the recipe.
    /// </summary>
    public long Id { get; set; }

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
    /// Gets or sets the date and time at which the recipe was soft-deleted, or null if the recipe
    /// has not been soft-deleted.
    /// </summary>
    public DateTime? Deleted { get; set; }

    /// <summary>
    /// Gets or sets the user who soft-deleted the recipe.
    /// </summary>
    public User? DeletedByUser { get; set; }

    /// <summary>
    /// Gets or sets the primary key of the user who soft-deleted the recipe.
    /// </summary>
    public long? DeletedByUserId { get; set; }

    /// <summary>
    /// Gets or sets the update count for concurrency control.
    /// </summary>
    [ConcurrencyCheck]
    public int UpdateCount { get; set; }

    /// <summary>
    /// Gets or sets the recipe's audit entries.
    /// </summary>
    public ICollection<RecipeAudit> Audits { get; set; } = [];

    /// <summary>
    /// Gets or sets the recipe's comments.
    /// </summary>
    public ICollection<Comment> Comments { get; set; } = [];
}
