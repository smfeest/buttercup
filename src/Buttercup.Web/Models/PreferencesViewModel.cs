using System.ComponentModel.DataAnnotations;
using Buttercup.Models;

namespace Buttercup.Web.Models;

public class PreferencesViewModel
{
    public PreferencesViewModel()
    {
    }

    public PreferencesViewModel(User user) => this.TimeZone = user.TimeZone;

    [Required(ErrorMessage = "Error_RequiredField")]
    public string? TimeZone { get; init; }
}
