using Microsoft.Extensions.Options;
using SendGrid;
using Xunit;

namespace Buttercup.Email;

public class SendGridClientAccessorTests
{
    [Fact]
    public void ProvidesTheSendGridClient()
    {
        var optionsAccessor = Options.Create(
            new EmailOptions { ApiKey = "sample-key", FromAddress = string.Empty });
        var accessor = new SendGridClientAccessor(optionsAccessor);
        Assert.IsType<SendGridClient>(accessor.SendGridClient);
    }
}
