using Buttercup.Email;
using Buttercup.TestUtils;
using Microsoft.Extensions.Localization;
using Moq;
using Xunit;

namespace Buttercup.Security;

public sealed class AuthenticationMailerTests
{
    private readonly Mock<IEmailSender> emailSenderMock = new();
    private readonly Mock<IStringLocalizer<AuthenticationMailer>> localizerMock = new();

    private readonly AuthenticationMailer authenticationMailer;

    public AuthenticationMailerTests() =>
        this.authenticationMailer = new(this.emailSenderMock.Object, this.localizerMock.Object);

    #region SendPasswordChangeNotification

    [Fact]
    public async Task SendPasswordChangeNotification_SendsEmail()
    {
        this.localizerMock
            .SetupLocalizedString("Subject_PasswordChangeNotification", "translated-subject")
            .SetupLocalizedString("Body_PasswordChangeNotification", "translated-body");

        await this.authenticationMailer.SendPasswordChangeNotification("user@example.com");

        this.emailSenderMock.Verify(
            x => x.Send("user@example.com", "translated-subject", "translated-body"));
    }

    #endregion

    #region SendPasswordResetLink

    [Fact]
    public async Task SendPasswordResetLink_SendsEmail()
    {
        const string PasswordResetUrl = "https://example.com/reset/password";

        this.localizerMock
            .SetupLocalizedString("Subject_PasswordResetLink", "translated-subject")
            .SetupLocalizedString(
                "Body_PasswordResetLink", new[] { PasswordResetUrl }, "translated-body");

        await this.authenticationMailer.SendPasswordResetLink(
            "user@example.com", PasswordResetUrl);

        this.emailSenderMock.Verify(
            x => x.Send("user@example.com", "translated-subject", "translated-body"));
    }

    #endregion
}
