using System.Security.Cryptography;

namespace Buttercup;

internal sealed class RandomNumberGeneratorFactory : IRandomNumberGeneratorFactory
{
    public RandomNumberGenerator Create() => RandomNumberGenerator.Create();
}
