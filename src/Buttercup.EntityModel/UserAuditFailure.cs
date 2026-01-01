namespace Buttercup.EntityModel;

/// <summary>
/// Specifies the reason an auditable user operation failed.
/// </summary>
public enum UserAuditFailure
{
    /// <summary>
    /// Indicates that an incorrect password was provided.
    /// </summary>
    IncorrectPassword,

    /// <summary>
    /// Indicates that the user currently has no password set.
    /// </summary>
    NoPasswordSet,

    /// <summary>
    /// Indicates that the user is deactivated.
    /// </summary>
    UserDeactivated,
}
