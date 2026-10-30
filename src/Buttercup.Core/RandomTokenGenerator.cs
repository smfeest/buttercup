using System.Buffers.Text;
using System.Security.Cryptography;

namespace Buttercup;

internal sealed class RandomTokenGenerator(RandomNumberGenerator randomNumberGenerator)
    : IRandomTokenGenerator
{
    private readonly RandomNumberGenerator randomNumberGenerator = randomNumberGenerator;

    public byte[] Generate(int size)
    {
        var bytes = new byte[size];
        this.randomNumberGenerator.GetBytes(bytes);
        return bytes;
    }

    public string GenerateUrlSafe(int size) => Base64Url.EncodeToString(this.Generate(size));
}
