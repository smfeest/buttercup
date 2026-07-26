using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace Buttercup.EntityModel;

/// <summary>
/// Represents an audit entry for a recipe.
/// </summary>
public sealed record RecipeAudit
{
    /// <summary>
    /// Gets or sets the primary key of the audit entry.
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
    /// Gets or sets the date and time of the action.
    /// </summary>
    public DateTime Time { get; set; }

    /// <summary>
    /// Gets or sets the action.
    /// </summary>
    public RecipeAction Action { get; set; }

    /// <summary>
    /// Gets or sets the user that performed the action.
    /// </summary>
    public User? Actor { get; set; }

    /// <summary>
    /// Gets or sets the primary key of the user that performed the action.
    /// </summary>
    public long? ActorId { get; set; }

    /// <summary>
    /// Gets or sets the IP address of the client through which the operation was initiated.
    /// </summary>
    public IPAddress? IpAddress { get; set; }

    /// <summary>
    /// Gets or sets the changes made to the recipe's attributes.
    /// </summary>
    [Column(TypeName = "json")]
    public RecipeChanges? Changes { get; set; }
}
