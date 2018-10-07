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
            var mockAuthenticationManager = new Mock<IAuthenticationManager>();

            var scheme = new AuthenticationScheme(
                CookieAuthenticationDefaults.AuthenticationScheme,
                null,
                typeof(CookieAuthenticationHandler));
            var ticket = new AuthenticationTicket(new ClaimsPrincipal(), null);
            var context = new CookieValidatePrincipalContext(
                new DefaultHttpContext(), scheme, new CookieAuthenticationOptions(), ticket);
            var result = Task.FromResult(new object());

            mockAuthenticationManager.Setup(x => x.ValidatePrincipal(context)).Returns(result);

            var cookieAuthenticationEventsHandler =
                new CookieAuthenticationEventsHandler(mockAuthenticationManager.Object);

            Assert.Equal(result, cookieAuthenticationEventsHandler.ValidatePrincipal(context));
        }

        #endregion
    }
}
