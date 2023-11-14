namespace Buttercup.EntityModel;

/// <summary>
/// Represents the exception that is thrown when a record is not found.
/// </summary>
public sealed class NotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class.
    /// </summary>
    public NotFoundException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class.
    /// </summary>
    /// <param name="message">
    /// The exception message.
    /// </param>
    public NotFoundException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class.
    /// </summary>
    /// <param name="message">
    /// The exception message.
    /// </param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception.
    /// </param>
    public NotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
