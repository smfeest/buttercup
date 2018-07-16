using Microsoft.Extensions.Options;
using SendGrid;

namespace Buttercup.Email
{
    /// <summary>
    /// The default implementation of <see cref="ISendGridClientAccessor" />.
    /// </summary>
    internal class SendGridClientAccessor : ISendGridClientAccessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SendGridClientAccessor" /> class.
        /// </summary>
        /// <param name="optionsAccessor">
        /// The email options accessor.
        /// </param>
        public SendGridClientAccessor(IOptions<EmailOptions> optionsAccessor) =>
            this.SendGridClient = new SendGridClient(optionsAccessor.Value.ApiKey);

        /// <inheritdoc />
        public ISendGridClient SendGridClient { get; }
    }
}
