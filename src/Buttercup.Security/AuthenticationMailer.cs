using Buttercup.Email;
using Microsoft.Extensions.Localization;

namespace Buttercup.Security;

internal sealed class AuthenticationMailer(
    IEmailSender emailSender, IStringLocalizer<AuthenticationMailer> localizer)
    : IAuthenticationMailer
{
    private readonly IEmailSender emailSender = emailSender;
    private readonly IStringLocalizer<AuthenticationMailer> localizer = localizer;

    public async Task SendPasswordChangeNotification(
        string email, CancellationToken cancellationToken) =>
        await this.emailSender.Send(
            email,
            this.localizer["Subject_PasswordChangeNotification"],
            this.localizer["Body_PasswordChangeNotification"],
            cancellationToken);

    public async Task SendPasswordResetLink(
        string email, string link, CancellationToken cancellationToken) =>
        await this.emailSender.Send(
            email,
            this.localizer["Subject_PasswordResetLink"],
            this.localizer["Body_PasswordResetLink", link],
            cancellationToken);
}
