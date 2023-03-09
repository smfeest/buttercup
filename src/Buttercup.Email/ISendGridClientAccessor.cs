using SendGrid;

namespace Buttercup.Email;

internal interface ISendGridClientAccessor
{
    ISendGridClient SendGridClient { get; }
}
