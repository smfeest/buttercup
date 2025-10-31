using System.Security.Cryptography;

namespace Buttercup;

/// <summary>
/// Defines the contract for the random number generator factory.
/// </summary>
public interface IRandomNumberGeneratorFactory
{
    /// <summary>
    /// Creates a new random number generator.
    /// </summary>
    /// <returns>
    /// The new random number generator.
    /// </returns>
    RandomNumberGenerator Create();
}
