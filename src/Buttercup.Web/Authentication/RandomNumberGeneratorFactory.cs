using System.Security.Cryptography;

namespace Buttercup.Web.Authentication;

public class RandomNumberGeneratorFactory : IRandomNumberGeneratorFactory
{
    public RandomNumberGenerator Create() => RandomNumberGenerator.Create();
}
