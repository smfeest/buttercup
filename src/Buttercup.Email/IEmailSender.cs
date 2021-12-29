namespace Buttercup.Email
{
    /// <summary>
    /// Defines the contract for the email sender.
    /// </summary>
    public interface IEmailSender
    {
        /// <summary>
        /// Sends an email.
        /// </summary>
        /// <param name="toAddress">
        /// The recipient's email address.
        /// </param>
        /// <param name="subject">
        /// The message subject.
        /// </param>
        /// <param name="body">
        /// The message body.
        /// </param>
        /// <returns>
        /// A task for the operation.
        /// </returns>
        Task Send(string toAddress, string subject, string body);
    }
}
