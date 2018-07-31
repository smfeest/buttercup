using System.Threading.Tasks;
using Buttercup.Email;
using Microsoft.Extensions.Localization;
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
            var context = new Context();

            context.MockLocalizer
                .SetupGet(x => x["Subject_PasswordChangeNotification"])
                .Returns(new LocalizedString(string.Empty, "translated-subject"));

            context.MockLocalizer
                .SetupGet(x => x["Body_PasswordChangeNotification"])
                .Returns(new LocalizedString(string.Empty, "translated-body"));

            await context.AuthenticationMailer.SendPasswordChangeNotification(
                "user@example.com");

            context.MockEmailSender.Verify(x => x.Send(
                "user@example.com", "translated-subject", "translated-body"));
        }

        #endregion

        #region SendPasswordResetLink

        [Fact]
        public async Task SendPasswordResetLinkSendsEmail()
        {
            var context = new Context();

            context.MockLocalizer
                .SetupGet(x => x["Subject_PasswordResetLink"])
                .Returns(new LocalizedString(string.Empty, "translated-subject"));

            context.MockLocalizer
                .SetupGet(x => x["Body_PasswordResetLink", "https://example.com/reset/password"])
                .Returns(new LocalizedString(string.Empty, "translated-body"));

            await context.AuthenticationMailer.SendPasswordResetLink(
                "user@example.com", "https://example.com/reset/password");

            context.MockEmailSender.Verify(x => x.Send(
                "user@example.com", "translated-subject", "translated-body"));
        }

        #endregion

        private class Context
        {
            public Context()
            {
                this.AuthenticationMailer = new AuthenticationMailer(
                    this.MockEmailSender.Object,
                    this.MockLocalizer.Object);
            }

            public AuthenticationMailer AuthenticationMailer { get; }

            public Mock<IEmailSender> MockEmailSender { get; } = new Mock<IEmailSender>();

            public Mock<IStringLocalizer<AuthenticationMailer>> MockLocalizer { get; } =
                new Mock<IStringLocalizer<AuthenticationMailer>>();
        }
    }
}
