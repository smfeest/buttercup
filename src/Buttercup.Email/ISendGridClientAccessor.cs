using SendGrid;

namespace Buttercup.Email
{
    /// <summary>
    /// Defines the contract for the SendGrid API client accessor.
    /// </summary>
    internal interface ISendGridClientAccessor
    {
        /// <summary>
        /// Gets the SendGrid API client.
        /// </summary>
        /// <value>
        /// The SendGrid API client.
        /// </value>
        ISendGridClient SendGridClient { get; }
    }
}
