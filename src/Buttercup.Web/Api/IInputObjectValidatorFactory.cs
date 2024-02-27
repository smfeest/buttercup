namespace Buttercup.Web.Api;

/// <summary>
/// Defines the contract for the input object validator factory.
/// </summary>
public interface IInputObjectValidatorFactory
{
    /// <summary>
    /// Creates a new input object validator.
    /// </summary>
    /// <typeparam name="T">
    /// The input object's runtime type.
    /// </typeparam>
    /// <param name="schema">
    /// The GraphQL schema.
    /// </param>
    /// <returns>
    /// The new validator.
    /// </returns>
    IInputObjectValidator<T> CreateValidator<T>(ISchema schema) where T : notnull;
}
