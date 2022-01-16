using System.ComponentModel.DataAnnotations;

namespace Buttercup.Web.Models;

public sealed record ResetPasswordViewModel
{
    [Required(ErrorMessage = "Error_RequiredField")]
    [DataType(DataType.Password)]
    [StringLength(int.MaxValue, MinimumLength = 6, ErrorMessage = "Error_PasswordTooShort")]
    public string Password { get; init; } = string.Empty;

    [Required(ErrorMessage = "Error_RequiredField")]
    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Error_PasswordsDoNotMatch")]
    public string ConfirmPassword { get; init; } = string.Empty;
}
