namespace Buttercup.Web.Authentication
{
    /// <summary>
    /// Represents the exception that is thrown when an invalid token is provided.
    /// </summary>
    public class InvalidTokenException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidTokenException"/> class.
        /// </summary>
        public InvalidTokenException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidTokenException"/> class.
        /// </summary>
        /// <param name="message">
        /// The exception message.
        /// </param>
        public InvalidTokenException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidTokenException"/> class.
        /// </summary>
        /// <param name="message">
        /// The exception message.
        /// </param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception.
        /// </param>
        public InvalidTokenException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
