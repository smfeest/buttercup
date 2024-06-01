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
    /// The name of the authorization policy that is satisfied if either the resource represents the
    /// current user, or the current user has the <see cref="RoleNames.Admin"/> role.
    /// </summary>
    public const string SelfOrAdmin = nameof(SelfOrAdmin);
}
