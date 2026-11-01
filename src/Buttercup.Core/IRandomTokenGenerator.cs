namespace Buttercup;

/// <summary>
/// Defines the contract for the random token generator.
/// </summary>
public interface IRandomTokenGenerator
{
    /// <summary>
    /// Randomly generates a new token.
    /// </summary>
    /// <param name="size">
    /// The size of the token in bytes.
    /// </param>
    /// <returns>
    /// The new token as a byte array.
    /// </returns>
    byte[] Generate(int size);

    /// <summary>
    /// Randomly generates a new Base64Url-encoded token.
    /// </summary>
    /// <param name="size">
    /// The size of the token in bytes, prior to Base64Url encoding.
    /// </param>
    /// <returns>
    /// The new token as a Base64Url string.
    /// </returns>
    string GenerateUrlSafe(int size);
}
