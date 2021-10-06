using Buttercup.Models;
using Xunit;

namespace Buttercup.Web.Models
{
    public class PreferencesViewModelTests
    {
        #region Constructor(User)

        [Fact]
        public void ConstructorCopiesValuesFromUser()
        {
            var user = new User { TimeZone = "user-time-zone" };

            var viewModel = new PreferencesViewModel(user);

            Assert.Equal(user.TimeZone, viewModel.TimeZone);
        }

        #endregion
    }
}
