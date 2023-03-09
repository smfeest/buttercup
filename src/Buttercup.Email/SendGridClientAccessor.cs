using Microsoft.Extensions.Options;
using SendGrid;

namespace Buttercup.Email;

internal class SendGridClientAccessor : ISendGridClientAccessor
{
    public SendGridClientAccessor(IOptions<EmailOptions> optionsAccessor)
    {
        var options = new SendGridClientOptions
        {
            ApiKey = optionsAccessor.Value.ApiKey,
            HttpErrorAsException = true,
        };

        this.SendGridClient = new SendGridClient(options);
    }

    public ISendGridClient SendGridClient { get; }
}
