using System.ComponentModel.DataAnnotations;

namespace Buttercup.Web.Models;

public sealed record SignInViewModel
{
    [Required(ErrorMessage = "Error_RequiredField")]
    [DataType(DataType.EmailAddress)]
    [EmailAddress(ErrorMessage = "Error_InvalidEmailAddress")]
    public string? Email { get; init; }

    [Required(ErrorMessage = "Error_RequiredField")]
    [DataType(DataType.Password)]
    public string? Password { get; init; }
}
