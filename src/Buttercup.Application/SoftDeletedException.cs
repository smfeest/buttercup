namespace Buttercup.Application;

/// <summary>
/// Represents the exception that is thrown when attempting to modify a soft-deleted record.
/// </summary>
public sealed class SoftDeletedException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SoftDeletedException"/> class.
    /// </summary>
    public SoftDeletedException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SoftDeletedException"/> class.
    /// </summary>
    /// <param name="message">
    /// The exception message.
    /// </param>
    public SoftDeletedException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SoftDeletedException"/> class.
    /// </summary>
    /// <param name="message">
    /// The exception message.
    /// </param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception.
    /// </param>
    public SoftDeletedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
