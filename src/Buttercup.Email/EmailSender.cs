using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Buttercup.Email;

internal sealed class EmailSender(
    ISendGridClientAccessor sendGridClientAccessor, IOptions<EmailOptions> optionsAccessor)
    : IEmailSender
{
    private readonly string fromAddress = optionsAccessor.Value.FromAddress;
    private readonly ISendGridClient sendGridClient = sendGridClientAccessor.SendGridClient;

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
