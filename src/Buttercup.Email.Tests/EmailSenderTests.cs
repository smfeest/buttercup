using System.Net;
using System.Net.Http.Json;
using Buttercup.Email.MailerSendApi;
using Microsoft.Extensions.Options;
using Xunit;

namespace Buttercup.Email;

public sealed class EmailSenderTests
{
    private const string ApiKey = "fake-api-key";
    private const string FromAddress = "from@example.com";
    private const string ToAddress = "to@example.com";
    private const string Subject = "message-subject";
    private const string Body = "message-body";

    #region Send

    [Fact]
    public async Task Send_PostsToMailerSendEmailEndpoint()
    {
        var calls = 0;

        using var httpMessageHandler = new FakeHttpMessageHandler(request =>
        {
            Assert.Equal(HttpMethod.Post, request.Method);
            Assert.Equal(new Uri("https://api.mailersend.com/v1/email"), request.RequestUri);

            calls++;
        });

        await Send(httpMessageHandler);

        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task Send_IncludesAuthorizationHeader()
    {
        using var httpMessageHandler = new FakeHttpMessageHandler(request =>
        {
            var authorizationHeader = request.Headers.Authorization;

            Assert.NotNull(authorizationHeader);
            Assert.Equal("Bearer", authorizationHeader.Scheme);
            Assert.Equal(ApiKey, authorizationHeader.Parameter);
        });

        await Send(httpMessageHandler);
    }

    [Fact]
    public async Task Send_IncludesEmailDetailsAsJsonContent()
    {
        using var httpMessageHandler = new FakeHttpMessageHandler(request =>
        {
            var jsonContent = Assert.IsType<JsonContent>(request.Content);
            var emailRequest = Assert.IsType<EmailRequestBody>(jsonContent.Value);

            Assert.Equal(FromAddress, emailRequest.From.Email);
            Assert.Equal(ToAddress, Assert.Single(emailRequest.To).Email);
            Assert.Equal(Subject, emailRequest.Subject);
            Assert.Equal(Body, emailRequest.Text);
        });

        await Send(httpMessageHandler);
    }

    [Fact]
    public async Task Send_ThrowsOnUnsuccessfulResponse()
    {
        using var httpMessageHandler = new FakeHttpMessageHandler(HttpStatusCode.Unauthorized);

        await Assert.ThrowsAsync<HttpRequestException>(() => Send(httpMessageHandler));
    }

    private static async Task Send(HttpMessageHandler httpMessageHandler)
    {
        using var httpClient = new HttpClient(httpMessageHandler);
        var options = new EmailOptions { ApiKey = ApiKey, FromAddress = FromAddress };

        var emailSender = new EmailSender(httpClient, Options.Create(options));

        await emailSender.Send(ToAddress, Subject, Body);
    }

    #endregion

    private sealed class FakeHttpMessageHandler(
        HttpStatusCode responseStatusCode, Action<HttpRequestMessage>? onSend = null)
        : HttpMessageHandler
    {
        public FakeHttpMessageHandler(Action<HttpRequestMessage> onSend)
            : this(HttpStatusCode.OK, onSend)
        {
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            onSend?.Invoke(request);
            return Task.FromResult(new HttpResponseMessage(responseStatusCode));
        }
    }
}
