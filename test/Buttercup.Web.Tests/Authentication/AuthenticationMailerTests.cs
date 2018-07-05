using System.Threading.Tasks;
using Buttercup.Email;
using Moq;
using Xunit;

namespace Buttercup.Web.Authentication
{
    public class AuthenticationMailerTests
    {
        #region SendPasswordChangeNotification

        [Fact]
        public async Task SendPasswordChangeNotificationSendsEmail()
        {
            var mockEmailSender = new Mock<IEmailSender>();

            await new AuthenticationMailer(mockEmailSender.Object).SendPasswordChangeNotification(
                "user@example.com");

            mockEmailSender.Verify(x => x.Send(
                "user@example.com",
                "Your password has been changed",
                It.Is<string>(body => body.Contains("Your Buttercup password has been changed"))));
        }

        #endregion

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
