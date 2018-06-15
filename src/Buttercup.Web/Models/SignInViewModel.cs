using System.ComponentModel.DataAnnotations;

namespace Buttercup.Web.Models
{
    public class SignInViewModel
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}
