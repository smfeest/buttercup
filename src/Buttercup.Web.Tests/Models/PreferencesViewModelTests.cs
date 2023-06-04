using Buttercup.TestUtils;
using Xunit;

namespace Buttercup.Web.Models;

public sealed class PreferencesViewModelTests
{
    #region Constructor(User)

    [Fact]
    public void ConstructorCopiesValuesFromUser()
    {
        var user = new ModelFactory().BuildUser();

        var viewModel = new PreferencesViewModel(user);

        Assert.Equal(user.TimeZone, viewModel.TimeZone);
    }

    #endregion
}
