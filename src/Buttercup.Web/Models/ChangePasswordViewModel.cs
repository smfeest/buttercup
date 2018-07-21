using System.ComponentModel.DataAnnotations;

namespace Buttercup.Web.Models
{
    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(int.MaxValue, MinimumLength = 6, ErrorMessage = "Password is too short")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords don't match")]
        public string ConfirmNewPassword { get; set; }
    }
}
