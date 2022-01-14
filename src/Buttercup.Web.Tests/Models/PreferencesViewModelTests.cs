using Buttercup.TestUtils;
using Xunit;

namespace Buttercup.Web.Models;

public class PreferencesViewModelTests
{
    #region Constructor(User)

    [Fact]
    public void ConstructorCopiesValuesFromUser()
    {
        var user = ModelFactory.CreateUser();

        var viewModel = new PreferencesViewModel(user);

        Assert.Equal(user.TimeZone, viewModel.TimeZone);
    }

    #endregion
}
