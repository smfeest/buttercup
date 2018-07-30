using System.Threading.Tasks;
using Buttercup.Email;
using Microsoft.Extensions.Localization;

namespace Buttercup.Web.Authentication
{
    public class AuthenticationMailer : IAuthenticationMailer
    {
        public AuthenticationMailer(
            IEmailSender emailSender, IStringLocalizer<AuthenticationMailer> localizer)
        {
            this.EmailSender = emailSender;
            this.Localizer = localizer;
        }

        public IEmailSender EmailSender { get; }

        public IStringLocalizer<AuthenticationMailer> Localizer { get; }

        public async Task SendPasswordChangeNotification(string email) =>
            await this.EmailSender.Send(
                email,
                this.Localizer["Subject_PasswordChangeNotification"],
                this.Localizer["Body_PasswordChangeNotification"]);

        public async Task SendPasswordResetLink(string email, string resetLink) =>
            await this.EmailSender.Send(
                email,
                this.Localizer["Subject_PasswordResetLink"],
                this.Localizer["Body_PasswordResetLink", resetLink]);
    }
}
