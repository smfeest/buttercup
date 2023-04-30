using System.ComponentModel.DataAnnotations;
using Buttercup.EntityModel;

namespace Buttercup.Web.Models;

public sealed record PreferencesViewModel
{
    public PreferencesViewModel()
    {
    }

    public PreferencesViewModel(User user) => this.TimeZone = user.TimeZone;

    [Required(ErrorMessage = "Error_RequiredField")]
    public string TimeZone { get; init; } = string.Empty;
}
