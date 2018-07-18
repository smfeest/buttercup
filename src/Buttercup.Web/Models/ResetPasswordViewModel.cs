using System.ComponentModel.DataAnnotations;

namespace Buttercup.Web.Models
{
    public class ResetPasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [StringLength(int.MaxValue, MinimumLength = 6, ErrorMessage = "Password is too short")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords don't match")]
        public string ConfirmPassword { get; set; }
    }
}
