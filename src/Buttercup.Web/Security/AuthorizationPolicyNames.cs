using Buttercup.Security;

namespace Buttercup.Web.Security;

/// <summary>
/// Provides constants for authorization policy names.
/// </summary>
public static class AuthorizationPolicyNames
{
    /// <summary>
    /// The name of the authorization policy that requires the <see cref="RoleNames.Admin"/> role.
    /// </summary>
    public const string AdminOnly = nameof(AdminOnly);

    /// <summary>
    /// The name of the authorization policy that requires that the user is authenticated and, if
    /// the resource represents a soft-deleted record, also has the <see cref="RoleNames.Admin"/>
    /// role.
    /// </summary>
    public const string AuthenticatedAndAdminWhenDeleted = nameof(AuthenticatedAndAdminWhenDeleted);

    /// <summary>
    /// The name of the authorization policy that is satisfied if either the resource represents a
    /// comment authored by the current user, or the current user has the <see
    /// cref="RoleNames.Admin"/> role.
    /// </summary>
    public const string CommentAuthorOrAdmin = nameof(CommentAuthorOrAdmin);

    /// <summary>
    /// The name of the GraphQL field authorization policy that is satisfied if either the parent
    /// object represents the current user, or the current user has the <see
    /// cref="RoleNames.Admin"/> role.
    /// </summary>
    public const string ParentResultSelfOrAdmin = nameof(ParentResultSelfOrAdmin);
}
