using System.ComponentModel.DataAnnotations;

namespace Buttercup.Web.Models
{
    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "Error_RequiredField")]
        [DataType(DataType.Password)]
        [StringLength(int.MaxValue, MinimumLength = 6, ErrorMessage = "Error_PasswordTooShort")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Error_PasswordsDoNotMatch")]
        public string ConfirmPassword { get; set; }
    }
}