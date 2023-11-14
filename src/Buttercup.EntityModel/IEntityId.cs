namespace Buttercup.EntityModel;

/// <summary>
/// Represents an entity type with an ID as its primary key.
/// </summary>
public interface IEntityId
{
    /// <summary>
    /// Gets the primary key of the entity.
    /// </summary>
    public long Id { get; }
}
