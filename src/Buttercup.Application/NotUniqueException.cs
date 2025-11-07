namespace Buttercup.Application;

/// <summary>
/// Represents the exception that is thrown when a unique constraint is violated.
/// </summary>
public sealed class NotUniqueException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotUniqueException"/> class.
    /// </summary>
    /// <param name="propertyName">
    /// The name of the property that contained the non-unique value.
    /// </param>
    public NotUniqueException(string propertyName) => this.PropertyName = propertyName;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotUniqueException"/> class.
    /// </summary>
    /// <param name="propertyName">
    /// The name of the property that contained the non-unique value.
    /// </param>
    /// <param name="message">
    /// The exception message.
    /// </param>
    public NotUniqueException(string propertyName, string message)
        : base(message) => this.PropertyName = propertyName;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotUniqueException"/> class.
    /// </summary>
    /// <param name="propertyName">
    /// The name of the property that contained the non-unique value.
    /// </param>
    /// <param name="message">
    /// The exception message.
    /// </param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception.
    /// </param>
    public NotUniqueException(string propertyName, string message, Exception innerException)
        : base(message, innerException) => this.PropertyName = propertyName;

    /// <summary>
    /// Gets the name of the property that contained the non-unique value.
    /// </summary>
    /// <value>
    /// The name of the property that contained the non-unique value.
    /// </value>
    public string PropertyName { get; }
}
