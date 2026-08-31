using Buttercup.EntityModel;

namespace Buttercup.TestUtils;

/// <summary>
/// Provides methods for comparing models in tests.
/// </summary>
public static class ModelCompare
{
    /// <summary>
    /// Indicates whether two <see cref="Comment"/> models are equal, excluding navigation
    /// properties.
    /// </summary>
    /// <param name="x">The first object to compare.</param>
    /// <param name="y">The second object to compare.</param>
    /// <returns>
    /// <b>true</b> if the two objects are equal after excluding navigation properties; <b>false</b>
    /// otherwise.
    /// </returns>
    public static bool EqualExcludingNavigationProperties(Comment x, Comment y)
    {
        static Comment ClearNavigationProperties(Comment comment) => comment with
        {
            Recipe = null,
            Author = null,
            DeletedByUser = null,
            Audits = Array.Empty<CommentAudit>(),
        };

        return ClearNavigationProperties(x) == ClearNavigationProperties(y);
    }

    /// <summary>
    /// Indicates whether two <see cref="CommentAudit"/> models are equal, excluding navigation
    /// properties.
    /// </summary>
    /// <param name="x">The first object to compare.</param>
    /// <param name="y">The second object to compare.</param>
    /// <returns>
    /// <b>true</b> if the two objects are equal after excluding navigation properties; <b>false</b>
    /// otherwise.
    /// </returns>
    public static bool EqualExcludingNavigationProperties(CommentAudit x, CommentAudit y)
    {
        static CommentAudit ClearNavigationProperties(CommentAudit audit) => audit with
        {
            Comment = null,
            Revision = null,
            Actor = null,
        };

        return ClearNavigationProperties(x) == ClearNavigationProperties(y);
    }

    /// <summary>
    /// Indicates whether two <see cref="CommentRevision"/> models are equal, excluding navigation
    /// properties.
    /// </summary>
    /// <param name="x">The first object to compare.</param>
    /// <param name="y">The second object to compare.</param>
    /// <returns>
    /// <b>true</b> if the two objects are equal after excluding navigation properties; <b>false</b>
    /// otherwise.
    /// </returns>
    public static bool EqualExcludingNavigationProperties(CommentRevision x, CommentRevision y)
    {
        static CommentRevision ClearNavigationProperties(CommentRevision revision) =>
            revision with { Comment = null };

        return ClearNavigationProperties(x) == ClearNavigationProperties(y);
    }

    /// <summary>
    /// Indicates whether two <see cref="Recipe"/> models are equal, excluding navigation
    /// properties.
    /// </summary>
    /// <param name="x">The first object to compare.</param>
    /// <param name="y">The second object to compare.</param>
    /// <returns>
    /// <b>true</b> if the two objects are equal after excluding navigation properties; <b>false</b>
    /// otherwise.
    /// </returns>
    public static bool EqualExcludingNavigationProperties(Recipe x, Recipe y)
    {
        static Recipe ClearNavigationProperties(Recipe recipe) => recipe with
        {
            CreatedByUser = null,
            ModifiedByUser = null,
            DeletedByUser = null,
            Audits = Array.Empty<RecipeAudit>(),
            Comments = Array.Empty<Comment>(),
        };

        return ClearNavigationProperties(x) == ClearNavigationProperties(y);
    }

    /// <summary>
    /// Indicates whether two <see cref="RecipeAudit"/> models are equal, excluding navigation
    /// properties.
    /// </summary>
    /// <param name="x">The first object to compare.</param>
    /// <param name="y">The second object to compare.</param>
    /// <returns>
    /// <b>true</b> if the two objects are equal after excluding navigation properties; <b>false</b>
    /// otherwise.
    /// </returns>
    public static bool EqualExcludingNavigationProperties(RecipeAudit x, RecipeAudit y)
    {
        static RecipeAudit ClearNavigationProperties(RecipeAudit audit) => audit with
        {
            Recipe = null,
            Revision = null,
            Actor = null,
        };

        return ClearNavigationProperties(x) == ClearNavigationProperties(y);
    }

    /// <summary>
    /// Indicates whether two <see cref="RecipeRevision"/> models are equal, excluding navigation
    /// properties.
    /// </summary>
    /// <param name="x">The first object to compare.</param>
    /// <param name="y">The second object to compare.</param>
    /// <returns>
    /// <b>true</b> if the two objects are equal after excluding navigation properties; <b>false</b>
    /// otherwise.
    /// </returns>
    public static bool EqualExcludingNavigationProperties(RecipeRevision x, RecipeRevision y)
    {
        static RecipeRevision ClearNavigationProperties(RecipeRevision revision) => revision with
        {
            Recipe = null,
            CreatedByUser = null,
        };

        return ClearNavigationProperties(x) == ClearNavigationProperties(y);
    }
}
