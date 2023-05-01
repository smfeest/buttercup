using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Buttercup.EntityModel;

/// <summary>
/// Represents a password reset token.
/// </summary>
public sealed record PasswordResetToken
{
    /// <summary>
    /// Gets or sets the token.
    /// </summary>
    [Key]
    [Column(TypeName = "char")]
    [StringLength(48)]
    public required string Token { get; set; }

    /// <summary>
    /// Gets or sets the associated user.
    /// </summary>
    public User? User { get; set; }

    /// <summary>
    /// Gets or sets the primary key of the associated user.
    /// </summary>
    public required long UserId { get; set; }

    /// <summary>
    /// Gets or sets the date and time at which the token was created.
    /// </summary>
    public required DateTime Created { get; set; }
}
