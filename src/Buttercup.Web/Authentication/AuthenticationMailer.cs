using Buttercup.Email;
using Microsoft.Extensions.Localization;

namespace Buttercup.Web.Authentication
{
    public class AuthenticationMailer : IAuthenticationMailer
    {
        private readonly IEmailSender emailSender;
        private readonly IStringLocalizer<AuthenticationMailer> localizer;

        public AuthenticationMailer(
            IEmailSender emailSender, IStringLocalizer<AuthenticationMailer> localizer)
        {
            this.emailSender = emailSender;
            this.localizer = localizer;
        }

        public async Task SendPasswordChangeNotification(string email) =>
            await this.emailSender.Send(
                email,
                this.localizer["Subject_PasswordChangeNotification"]!,
                this.localizer["Body_PasswordChangeNotification"]!);

        public async Task SendPasswordResetLink(string email, string link) =>
            await this.emailSender.Send(
                email,
                this.localizer["Subject_PasswordResetLink"]!,
                this.localizer["Body_PasswordResetLink", link]!);
    }
}
