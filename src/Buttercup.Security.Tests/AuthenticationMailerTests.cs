using Buttercup.Email;
using Buttercup.TestUtils;
using Moq;
using Xunit;

namespace Buttercup.Security;

public sealed class AuthenticationMailerTests
{
    private readonly Mock<IEmailSender> emailSenderMock = new();
    private readonly DictionaryLocalizer<AuthenticationMailer> localizer = new();

    private readonly AuthenticationMailer authenticationMailer;

    public AuthenticationMailerTests() =>
        this.authenticationMailer = new(this.emailSenderMock.Object, this.localizer);

    #region SendPasswordChangeNotification

    [Fact]
    public async Task SendPasswordChangeNotification_SendsEmail()
    {
        this.localizer
            .Add("Subject_PasswordChangeNotification", "translated-subject")
            .Add("Body_PasswordChangeNotification", "translated-body");

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

        this.localizer
            .Add("Subject_PasswordResetLink", "translated-subject")
            .Add("Body_PasswordResetLink", "translated-body / {0}");

        await this.authenticationMailer.SendPasswordResetLink(
            "user@example.com", PasswordResetUrl);

        this.emailSenderMock.Verify(
            x => x.Send(
                "user@example.com", "translated-subject", $"translated-body / {PasswordResetUrl}"));
    }

    #endregion
}
