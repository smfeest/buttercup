using System.Buffers.Text;
using System.Security.Cryptography;
using Moq;
using Xunit;

namespace Buttercup;

public sealed class RandomTokenGeneratorTests
{
    private readonly Mock<RandomNumberGenerator> randomNumberGeneratorMock = new();
    private readonly RandomTokenGenerator randomTokenGenerator;

    public RandomTokenGeneratorTests() =>
        this.randomTokenGenerator = new(this.randomNumberGeneratorMock.Object);

    #region GenerateUrlSafe

    [Theory]
    [InlineData(3)]
    [InlineData(6)]
    public void GenerateUrlSafe_ReturnsBase64UrlEncodedRandomTokenOfRequestedLength(int length)
    {
        var expectedBytes = RandomNumberGenerator.GetBytes(length);

        this.randomNumberGeneratorMock
            .Setup(x => x.GetBytes(It.Is<byte[]>(array => array.Length == length)))
            .Callback((byte[] bytes) => Array.Copy(expectedBytes, bytes, length));

        Assert.Equal(
            Base64Url.EncodeToString(expectedBytes),
            this.randomTokenGenerator.GenerateUrlSafe(length));
    }

    #endregion
}
