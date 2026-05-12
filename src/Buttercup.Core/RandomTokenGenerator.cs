using System.Security.Cryptography;

namespace Buttercup;

internal sealed class RandomTokenGenerator(RandomNumberGenerator randomNumberGenerator)
    : IRandomTokenGenerator
{
    private readonly RandomNumberGenerator randomNumberGenerator = randomNumberGenerator;

    public string Generate(int n)
    {
        var bytes = new byte[n * 3];
        this.randomNumberGenerator.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace('+', '-').Replace('/', '_');
    }
}
