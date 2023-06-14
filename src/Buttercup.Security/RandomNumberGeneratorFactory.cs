using System.Security.Cryptography;

namespace Buttercup.Security;

internal sealed class RandomNumberGeneratorFactory : IRandomNumberGeneratorFactory
{
    public RandomNumberGenerator Create() => RandomNumberGenerator.Create();
}
