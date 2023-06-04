using System.Security.Cryptography;

namespace Buttercup.Web.Authentication;

public sealed class RandomNumberGeneratorFactory : IRandomNumberGeneratorFactory
{
    public RandomNumberGenerator Create() => RandomNumberGenerator.Create();
}
