using System.Threading.Tasks;
using Buttercup.Email;

namespace Buttercup.Web.Authentication
{
    public class AuthenticationMailer : IAuthenticationMailer
    {
        public AuthenticationMailer(IEmailSender emailSender) => this.EmailSender = emailSender;

        public IEmailSender EmailSender { get; }

        public async Task SendPasswordResetLink(string email, string resetLink)
        {
            var body = $@"Please use this link to reset your Buttercup password:

{resetLink}

If you no longer wish to reset your password you can safely ignore this
email and your password will remaining unchanged. The link will expire
in 24 hours.";

            await this.EmailSender.Send(email, "Link to reset your password", body);
        }
    }
}
