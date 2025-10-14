using Xunit;

namespace Buttercup.Email;

public sealed class FakeHttpMessageHandler() : HttpMessageHandler
{
    public Queue<Func<HttpRequestMessage, HttpResponseMessage>> Callbacks { get; } = new();

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!this.Callbacks.TryDequeue(out var callback))
        {
            Assert.Fail("Unexpected HTTP request");
        }
        return Task.FromResult(callback(request));
    }
}
