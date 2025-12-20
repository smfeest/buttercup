namespace Buttercup.EntityModel;

/// <summary>
/// Specifies the type of operation performed on a user.
/// </summary>
public enum UserOperation
{
    /// <summary>
    /// Indicates that the user changed their password.
    /// </summary>
    ChangePassword,

    /// <summary>
    /// Indicates that the user was created.
    /// </summary>
    Create,

    /// <summary>
    /// Indicates that the user reset their password.
    /// </summary>
    ResetPassword,
}
