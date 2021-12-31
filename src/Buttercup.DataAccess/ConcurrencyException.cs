namespace Buttercup.DataAccess;

/// <summary>
/// Represents the exception that is thrown when a concurrency error occurs.
/// </summary>
public class ConcurrencyException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConcurrencyException"/> class.
    /// </summary>
    public ConcurrencyException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConcurrencyException"/> class.
    /// </summary>
    /// <param name="message">
    /// The exception message.
    /// </param>
    public ConcurrencyException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConcurrencyException"/> class.
    /// </summary>
    /// <param name="message">
    /// The exception message.
    /// </param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception.
    /// </param>
    public ConcurrencyException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
