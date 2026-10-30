namespace Buttercup;

/// <summary>
/// Defines the contract for the random token generator.
/// </summary>
public interface IRandomTokenGenerator
{
    /// <summary>
    /// Generates a new token.
    /// </summary>
    /// <param name="size">
    /// The size of the token in bytes, prior to Base64Url encoding.
    /// </param>
    /// <returns>
    /// The new token as a Base64Url string.
    /// </returns>
    string Generate(int size);
}
