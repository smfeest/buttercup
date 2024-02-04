using System.Collections.ObjectModel;

namespace Buttercup.Application.Validation;

/// <summary>
/// Represents the exception that is thrown when an object fails validation.
/// </summary>
public sealed class ValidationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// </summary>
    /// <param name="validationErrors">
    /// The validation errors.
    /// </param>
    public ValidationException(ReadOnlyCollection<ValidationError> validationErrors) =>
        this.ValidationErrors = validationErrors;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// </summary>
    /// <param name="message">
    /// The exception message.
    /// </param>
    /// <param name="validationErrors">
    /// The validation errors.
    /// </param>
    public ValidationException(string message, ReadOnlyCollection<ValidationError> validationErrors)
        : base(message) =>
        this.ValidationErrors = validationErrors;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// </summary>
    /// <param name="message">
    /// The exception message.
    /// </param>
    /// <param name="innerException">
    /// The exception that is the cause of the current exception.
    /// </param>
    /// <param name="validationErrors">
    /// The validation errors.
    /// </param>
    public ValidationException(
        string message,
        Exception? innerException,
        ReadOnlyCollection<ValidationError> validationErrors)
        : base(message, innerException) =>
        this.ValidationErrors = validationErrors;

    /// <summary>
    /// Gets the validation errors.
    /// </summary>
    public ReadOnlyCollection<ValidationError> ValidationErrors { get; }
}
