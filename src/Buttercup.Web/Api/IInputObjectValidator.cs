namespace Buttercup.Web.Api;

/// <summary>
/// Defines the contract for the input object validator.
/// </summary>
/// <typeparam name="T">
/// The input object's runtime type.
/// </typeparam>
public interface IInputObjectValidator<T> where T : notnull
{
    /// <summary>
    /// Validates an instance of <typeparamref name="T"/>.
    /// </summary>
    /// <param name="instance">
    /// The input object to validate.
    /// </param>
    /// <param name="basePath">
    /// The field path for the input object.
    /// </param>
    /// <param name="validationErrors">
    /// The collection to add validation errors to.
    /// </param>
    /// <returns>
    /// <b>true</b> if the <paramref name="instance"/> has no validation errors, <b>false</b>
    /// otherwise.
    /// </returns>
    bool Validate(
        T instance, string[] basePath, ICollection<InputObjectValidationError> validationErrors);
}
