using System.Net.Http.Json;
using Buttercup.Email.MailerSendApi;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace Buttercup.Email;

internal sealed class EmailSender : IEmailSender
{
    private readonly HttpClient httpClient;
    private readonly string fromAddress;

    public EmailSender(HttpClient httpClient, IOptions<EmailOptions> optionsAccessor)
    {
        this.fromAddress = optionsAccessor.Value.FromAddress;
        this.httpClient = httpClient;

        httpClient.BaseAddress = new("https://api.mailersend.com/v1/");
        httpClient.DefaultRequestHeaders.Add(
            HeaderNames.Authorization, $"Bearer {optionsAccessor.Value.ApiKey}");
    }

    public async Task Send(string toAddress, string subject, string body)
    {
        var message = new EmailRequestBody(new(this.fromAddress), [new(toAddress)], subject, body);
        using var response = await this.httpClient.PostAsJsonAsync("email", message);
        response.EnsureSuccessStatusCode();
    }
}
