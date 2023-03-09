using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Buttercup.Email;

internal class EmailSender : IEmailSender
{
    private readonly string fromAddress;

    private readonly ISendGridClient sendGridClient;

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
