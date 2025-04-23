using System.Diagnostics.CodeAnalysis;
using Buttercup.EntityModel;

namespace Buttercup.Security;

/// <summary>
/// Represents the result of a password authentication attempt.
/// </summary>
public sealed record PasswordAuthenticationResult
{
    /// <summary>
    /// Initializes a new instance representing a successful attempt.
    /// </summary>
    /// <param name="user">The authenticated user.</param>
    public PasswordAuthenticationResult(User user) => this.User = user;

    /// <summary>
    /// Initializes a new instance representing an unsuccessful attempt.
    /// </summary>
    /// <param name="failure">The cause of the failure.</param>
    public PasswordAuthenticationResult(PasswordAuthenticationFailure failure) =>
        this.Failure = failure;

    /// <summary>
    /// The cause of failure, or null if the authentication attempt was successful.
    /// </summary>
    public PasswordAuthenticationFailure? Failure { get; }

    /// <summary>
    /// <b>true</b> if the user was successfully authenticated; otherwise, <b>false</b>.
    /// </summary>
    [MemberNotNullWhen(false, nameof(Failure))]
    [MemberNotNullWhen(true, nameof(User))]
    public bool IsSuccess => this.User is not null;

    /// <summary>
    /// The authenticated user, or a null reference if the authentication attempt was unsuccessful.
    /// </summary>
    public User? User { get; }
}
