namespace Buttercup.EntityModel;

/// <summary>
/// Specifies the type of action performed on a recipe.
/// </summary>
public enum RecipeAction
{
    /// <summary>
    /// Indicates that the recipe was created.
    /// </summary>
    Create,

    /// <summary>
    /// Indicates that the recipe was soft-deleted.
    /// </summary>
    Delete,

    /// <summary>
    /// Indicates that the recipe was modified.
    /// </summary>
    Modify,
}
