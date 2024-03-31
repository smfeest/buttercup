namespace Buttercup.EntityModel;

/// <summary>
/// Represents a soft-deletable entity type.
/// </summary>
public interface ISoftDeletable
{
    /// <summary>
    /// Gets or sets the date and time at which the entity was soft-deleted, or null if the entity
    /// has not been soft-deleted.
    /// </summary>
    public DateTime? Deleted { get; set; }
}
