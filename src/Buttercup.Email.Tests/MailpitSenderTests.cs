using System.Net;
using System.Net.Http.Json;
using Buttercup.Email.Mailpit;
using Buttercup.TestUtils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;
using Xunit;

namespace Buttercup.Email;

public sealed class MailpitSenderTests : IDisposable
{
    private readonly FakeHttpMessageHandler httpMessageHandler = new();
    private readonly HttpClient httpClient;
    private readonly FakeLogger<MailpitSender> logger = new();
    private readonly EmailOptions options = new()
    {
        FromAddress = "from@example.com",
        MailpitServer = new Uri("http://mailpit-host:1234")
    };

    private readonly MailpitSender mailpitSender;

    public MailpitSenderTests()
    {
        this.httpClient = new(this.httpMessageHandler);
        this.mailpitSender = new(this.httpClient, this.logger, Options.Create(this.options));
    }

    public void Dispose() => this.httpClient.Dispose();

    #region Send

    [Fact]
    public async Task Send_PostsToSendEndpoint()
    {
        this.httpMessageHandler.Callbacks.Enqueue(request =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal(
                new Uri("http://mailpit-host:1234/api/v1/send"),
                request.RequestUri);

            var jsonContent = Assert.IsType<JsonContent>(request.Content);
            var requestBody = Assert.IsType<SendRequestBody>(jsonContent.Value);
            Assert.Equal("from@example.com", requestBody.From.Email);
            Assert.Equal("to@example.com", Assert.Single(requestBody.To).Email);
            Assert.Equal("test-message-subject", requestBody.Subject);
            Assert.Equal("test-message-body", requestBody.Text);

            return SuccessResponse();
        });

        await this.Send();
    }

    [Fact]
    public async Task Send_ThrowsOnUnsuccessfulResponse()
    {
        this.httpMessageHandler.Callbacks.Enqueue(_ => new(HttpStatusCode.Unauthorized));

        await Assert.ThrowsAsync<HttpRequestException>(this.Send);
    }

    [Fact]
    public async Task Send_ThrowsOnMissingResponseBody()
    {
        this.httpMessageHandler.Callbacks.Enqueue(
            _ => new(HttpStatusCode.OK) { Content = JsonContent.Create<object?>(null) });

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(this.Send);
        Assert.Equal("No response body from send endpoint", exception.Message);
    }

    [Fact]
    public async Task Send_LogsMessageSent()
    {
        this.httpMessageHandler.Callbacks.Enqueue(_ => SuccessResponse());

        await this.Send();

        LogAssert.SingleEntry(this.logger)
            .HasId(1)
            .HasLevel(LogLevel.Information)
            .HasMessage($"Sent message test-message-id to to@example.com");
    }

    private Task Send() => this.mailpitSender.Send(
        "to@example.com", "test-message-subject", "test-message-body");

    private static HttpResponseMessage SuccessResponse() => new(HttpStatusCode.OK)
    {
        Content = JsonContent.Create(new SendResponseBody("test-message-id")),
    };

    #endregion
}
