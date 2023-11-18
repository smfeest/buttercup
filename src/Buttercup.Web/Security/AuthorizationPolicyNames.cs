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
    public const string AdminOnly = "AdminOnly";
}
