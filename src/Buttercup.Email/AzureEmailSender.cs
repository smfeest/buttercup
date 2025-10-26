using Azure;
using Azure.Communication.Email;
using Microsoft.Extensions.Options;

namespace Buttercup.Email;

internal sealed class AzureEmailSender(
    EmailClient emailClient, IOptions<EmailOptions> optionsAccessor)
    : IEmailSender
{
    private readonly EmailClient emailClient = emailClient;
    private readonly string fromAddress = optionsAccessor.Value.FromAddress;

    public Task Send(string toAddress, string subject, string body) =>
        this.emailClient.SendAsync(
            WaitUntil.Started,
            new(this.fromAddress, toAddress, new(subject) { PlainText = body }));
}
