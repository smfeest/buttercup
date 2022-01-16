using System.ComponentModel.DataAnnotations;

namespace Buttercup.Web.Models;

public sealed record RequestPasswordResetViewModel
{
    [Required(ErrorMessage = "Error_RequiredField")]
    [DataType(DataType.EmailAddress)]
    [EmailAddress(ErrorMessage = "Error_InvalidEmailAddress")]
    public string Email { get; init; } = string.Empty;
}
