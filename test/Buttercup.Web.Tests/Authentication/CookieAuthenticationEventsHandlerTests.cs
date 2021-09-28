using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace Buttercup.Web.Authentication
{
    public class CookieAuthenticationEventsHandlerTests
    {
        #region ValidatePrincipal

        [Fact]
        public void ValidatePrincipalDelegatesToAuthenticationManager()
        {
            var scheme = new AuthenticationScheme(
                CookieAuthenticationDefaults.AuthenticationScheme,
                null,
                typeof(CookieAuthenticationHandler));
            var ticket = new AuthenticationTicket(new(), string.Empty);
            var context = new CookieValidatePrincipalContext(
                new DefaultHttpContext(), scheme, new(), ticket);
            var expectedResult = Task.FromResult(new object());

            var authenticationManager = Mock.Of<IAuthenticationManager>(
                x => x.ValidatePrincipal(context) == expectedResult);

            var actualResult = new CookieAuthenticationEventsHandler(authenticationManager)
                .ValidatePrincipal(context);

            Assert.Equal(expectedResult, actualResult);
        }

        #endregion
    }
}
