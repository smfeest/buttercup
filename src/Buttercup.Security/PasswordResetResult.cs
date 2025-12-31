using System.Diagnostics.CodeAnalysis;
using Buttercup.EntityModel;

namespace Buttercup.Security;

/// <summary>
/// Represents the result, or expected result, of a password reset.
/// </summary>
public sealed record PasswordResetResult
{
    /// <summary>
    /// Initializes a new instance representing success.
    /// </summary>
    /// <param name="user">The authenticated user.</param>
    public PasswordResetResult(User user) => this.User = user;

    /// <summary>
    /// Initializes a new instance representing failure.
    /// </summary>
    /// <param name="reason">The reason for failure.</param>
    public PasswordResetResult(PasswordResetFailure reason) => this.Failure = reason;

    /// <summary>
    /// The cause of the failure, or null if successful.
    /// </summary>
    public PasswordResetFailure? Failure { get; }

    /// <summary>
    /// <b>true</b> if the password reset was, or is expected to be, successful; otherwise,
    /// <b>false</b>.
    /// </summary>
    [MemberNotNullWhen(true, nameof(User))]
    [MemberNotNullWhen(false, nameof(Failure))]
    public bool IsSuccess => this.User is not null;

    /// <summary>
    /// The user affected by the password reset, or a null reference if the password reset token is
    /// invalid.
    /// </summary>
    public User? User { get; }
}

