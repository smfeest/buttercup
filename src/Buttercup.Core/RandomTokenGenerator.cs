using System.Buffers.Text;
using System.Security.Cryptography;

namespace Buttercup;

internal sealed class RandomTokenGenerator(RandomNumberGenerator randomNumberGenerator)
    : IRandomTokenGenerator
{
    private readonly RandomNumberGenerator randomNumberGenerator = randomNumberGenerator;

    public string Generate(int size)
    {
        var bytes = new byte[size];
        this.randomNumberGenerator.GetBytes(bytes);
        return Base64Url.EncodeToString(bytes);
    }
}
