using System.ComponentModel.DataAnnotations;

namespace Buttercup.Web.Models
{
    public class RequestPasswordResetViewModel
    {
        [Required]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        public string Email { get; set; }
    }
}
