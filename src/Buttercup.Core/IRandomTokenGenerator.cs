namespace Buttercup;

/// <summary>
/// Defines the contract for the random token generator.
/// </summary>
public interface IRandomTokenGenerator
{
    /// <summary>
    /// Generates a new token.
    /// </summary>
    /// <param name="n">
    /// The length factor.
    /// </param>
    /// <returns>
    /// The randomly generated token as a URL-safe Base64 string of 4 × <paramref name="n"/>
    /// characters.
    /// </returns>
    string Generate(int n);
}
