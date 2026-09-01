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
    /// Gets or sets the recipe revision.
    /// </summary>
    /// <remarks>
    /// This property is specified if and only if <see cref="Action"/> is <see
    /// cref="RecipeAction.Create"/> or <see cref="RecipeAction.Update"/>.
    /// </remarks>
    public RecipeRevision? Revision { get; set; }

    /// <summary>
    /// Gets or sets the primary key of the recipe revision.
    /// </summary>
    /// <remarks>
    /// This property is specified if and only if <see cref="Action"/> is <see
    /// cref="RecipeAction.Create"/> or <see cref="RecipeAction.Update"/>.
    /// </remarks>
    public long? RevisionId { get; set; }

    /// <summary>
    /// Gets or sets the user that performed the action.
    /// </summary>
    public User? Actor { get; set; }

    /// <summary>
    /// Gets or sets the primary key of the user that performed the action.
    /// </summary>
    public long? ActorId { get; set; }

    /// <summary>
    /// Gets or sets the IP address of the user that performed the action.
    /// </summary>
    public IPAddress? IpAddress { get; set; }
}
