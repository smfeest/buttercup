using System.Threading.Tasks;
using Buttercup.Email;
using Moq;
using Xunit;

namespace Buttercup.Web.Authentication
{
    public class AuthenticationMailerTests
    {
        #region SendPasswordResetLink

        [Fact]
        public async Task SendPasswordResetLinkSendsEmail()
        {
            var mockEmailSender = new Mock<IEmailSender>();

            await new AuthenticationMailer(mockEmailSender.Object).SendPasswordResetLink(
                "user@example.com", "https://example.com/reset/password");

            mockEmailSender.Verify(x => x.Send(
                "user@example.com",
                "Link to reset your password",
                It.Is<string>(body => body.Contains("https://example.com/reset/password"))));
        }

        #endregion
    }
}
