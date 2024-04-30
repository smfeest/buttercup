using Buttercup.TestUtils;
using Xunit;

namespace Buttercup.Web.Models.Account;

public sealed class PreferencesViewModelTests
{
    #region Constructor(User)

    [Fact]
    public void Constructor_CopiesValuesFromUser()
    {
        var user = new ModelFactory().BuildUser();

        var viewModel = new PreferencesViewModel(user);

        Assert.Equal(user.TimeZone, viewModel.TimeZone);
    }

    #endregion
}
