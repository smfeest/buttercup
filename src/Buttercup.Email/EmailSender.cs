using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Buttercup.Email
{
    /// <summary>
    /// The default implementation of <see cref="IEmailSender" />.
    /// </summary>
    internal class EmailSender : IEmailSender
    {
        private readonly string fromAddress;

        private readonly ISendGridClient sendGridClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailSender" /> class.
        /// </summary>
        /// <param name="sendGridClientAccessor">
        /// The SendGrid client accessor.
        /// </param>
        /// <param name="optionsAccessor">
        /// The email options accessor.
        /// </param>
        public EmailSender(
            ISendGridClientAccessor sendGridClientAccessor, IOptions<EmailOptions> optionsAccessor)
        {
            if (string.IsNullOrEmpty(optionsAccessor.Value.FromAddress))
            {
                throw new ArgumentException(
                    "FromAddress must not be null or empty",
                    nameof(optionsAccessor));
            }

            this.sendGridClient = sendGridClientAccessor.SendGridClient;
            this.fromAddress = optionsAccessor.Value.FromAddress;
        }

        /// <inheritdoc />
        public Task Send(string toAddress, string subject, string body)
        {
            var message = new SendGridMessage()
            {
                From = new(this.fromAddress),
                Subject = subject,
                PlainTextContent = body,
            };
            message.AddTo(new EmailAddress(toAddress));

            return this.sendGridClient.SendEmailAsync(message);
        }
    }
}
