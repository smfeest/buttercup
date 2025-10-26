using System.Net.Http.Json;
using Buttercup.Email.Mailpit;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Buttercup.Email;

internal sealed partial class MailpitSender(
    HttpClient httpClient, ILogger<MailpitSender> logger, IOptions<EmailOptions> optionsAccessor)
    : IEmailSender
{
    private readonly EmailOptions options = optionsAccessor.Value;
    private readonly HttpClient httpClient = httpClient;
    private readonly ILogger<MailpitSender> logger = logger;

    public async Task Send(string toAddress, string subject, string body)
    {
        var response = await this.httpClient.PostAsJsonAsync(
            new Uri(this.options.MailpitServer, "/api/v1/send"),
            new(new(this.options.FromAddress), [new(toAddress)], subject, body),
            SerializerContext.Default.SendRequestBody);

        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadFromJsonAsync(
            SerializerContext.Default.SendResponseBody) ??
            throw new InvalidOperationException("No response body from send endpoint");

        this.LogMessageSent(responseBody.Id, toAddress);
    }

    [LoggerMessage(
        EventId = 1,
        EventName = "MessageSent",
        Level = LogLevel.Information,
        Message = "Sent message {messageId} to {toAddress}")]
    private partial void LogMessageSent(string messageId, string toAddress);
}
