namespace Buttercup.Application.Validation;

/// <summary>
/// Defines the contract for the validator.
/// </summary>
/// <typeparam name="T">
/// The type of object validated by the validator.
/// </typeparam>
public interface IValidator<T> where T : notnull
{
    /// <summary>
    /// Validates an instance of <typeparamref name="T"/>.
    /// </summary>
    /// <param name="instance">
    /// The object to validate.
    /// </param>
    /// <param name="validationErrors">
    /// The collection to which validation errors are to be added.
    /// </param>
    /// <returns>
    /// <b>true</b> if the <paramref name="instance"/> has no validation errors, <b>false</b>
    /// otherwise.
    /// </returns>
    bool Validate(T instance, ICollection<ValidationError> validationErrors);
}
