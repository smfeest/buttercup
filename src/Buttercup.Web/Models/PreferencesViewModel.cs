using Buttercup.Models;

namespace Buttercup.Web.Models;

public class PreferencesViewModel
{
    public PreferencesViewModel()
    {
    }

    public PreferencesViewModel(User user) => this.TimeZone = user.TimeZone;

    public string? TimeZone { get; init; }
}
