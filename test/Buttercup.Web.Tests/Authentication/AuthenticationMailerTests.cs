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
            var fixture = new AuthenticationMailerFixture();

            fixture.MockLocalizer
                .SetupGet(x => x["Subject_PasswordChangeNotification"])
                .Returns(new LocalizedString(string.Empty, "translated-subject"));

            fixture.MockLocalizer
                .SetupGet(x => x["Body_PasswordChangeNotification"])
                .Returns(new LocalizedString(string.Empty, "translated-body"));

            await fixture.AuthenticationMailer.SendPasswordChangeNotification(
                "user@example.com");

            fixture.MockEmailSender.Verify(x => x.Send(
                "user@example.com", "translated-subject", "translated-body"));
        }

        #endregion

        #region SendPasswordResetLink

        [Fact]
        public async Task SendPasswordResetLinkSendsEmail()
        {
            var fixture = new AuthenticationMailerFixture();

            fixture.MockLocalizer
                .SetupGet(x => x["Subject_PasswordResetLink"])
                .Returns(new LocalizedString(string.Empty, "translated-subject"));

            fixture.MockLocalizer
                .SetupGet(x => x["Body_PasswordResetLink", "https://example.com/reset/password"])
                .Returns(new LocalizedString(string.Empty, "translated-body"));

            await fixture.AuthenticationMailer.SendPasswordResetLink(
                "user@example.com", "https://example.com/reset/password");

            fixture.MockEmailSender.Verify(x => x.Send(
                "user@example.com", "translated-subject", "translated-body"));
        }

        #endregion

        private class AuthenticationMailerFixture
        {
            public AuthenticationMailerFixture()
            {
                this.AuthenticationMailer = new(
                    this.MockEmailSender.Object,
                    this.MockLocalizer.Object);
            }

            public AuthenticationMailer AuthenticationMailer { get; }

            public Mock<IEmailSender> MockEmailSender { get; } = new();

            public Mock<IStringLocalizer<AuthenticationMailer>> MockLocalizer { get; } = new();
        }
    }
}
