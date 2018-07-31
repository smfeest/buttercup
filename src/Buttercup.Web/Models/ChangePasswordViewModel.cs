using System.ComponentModel.DataAnnotations;

namespace Buttercup.Web.Models
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Error_RequiredField")]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [Required(ErrorMessage = "Error_RequiredField")]
        [DataType(DataType.Password)]
        [StringLength(int.MaxValue, MinimumLength = 6, ErrorMessage = "Error_PasswordTooShort")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Error_PasswordsDoNotMatch")]
        public string ConfirmNewPassword { get; set; }
    }
}
