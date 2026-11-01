using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Buttercup.EntityModel;

/// <summary>
/// Represents a password reset token.
/// </summary>
[Index(nameof(Token), IsUnique = true)]
[Index(nameof(HashedToken), IsUnique = true)]
public sealed record PasswordResetToken
{
    /// <summary>
    /// Gets or sets the token.
    /// </summary>
    [StringLength(48)]
    public required string Token { get; set; }

    /// <summary>
    /// Gets or sets the hashed token.
    /// </summary>
    public required byte[]? HashedToken { get; set; }

    /// <summary>
    /// Gets or sets the associated user.
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// Gets or sets the primary key of the associated user.
    /// </summary>
    [Key]
    public required long UserId { get; set; }

    /// <summary>
    /// Gets or sets the date and time at which the token was created.
    /// </summary>
    public required DateTime Created { get; set; }
}
