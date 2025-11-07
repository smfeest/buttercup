using System.Security.Cryptography;
using Moq;
using Xunit;

namespace Buttercup;

public sealed class RandomTokenGeneratorTests
{
    private readonly Mock<RandomNumberGenerator> randomNumberGeneratorMock = new();
    private readonly RandomTokenGenerator randomTokenGenerator;

    public RandomTokenGeneratorTests()
    {
        var randomNumberGeneratorFactory = Mock.Of<IRandomNumberGeneratorFactory>(
            x => x.Create() == this.randomNumberGeneratorMock.Object);

        this.randomTokenGenerator = new(randomNumberGeneratorFactory);
    }

    #region Generate

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    public void Generate_Uses3nRandomBytes(int n)
    {
        this.randomTokenGenerator.Generate(n);

        this.randomNumberGeneratorMock
            .Verify(x => x.GetBytes(It.Is<byte[]>(bytes => bytes.Length == (3 * n))));
    }

    [Fact]
    public void Generate_ReturnsUrlSafeBase64()
    {
        this.randomNumberGeneratorMock
            .Setup(x => x.GetBytes(It.IsAny<byte[]>()))
            .Callback((byte[] bytes) =>
            {
                var generatedBytes = new byte[]
                {
                    0xd1, 0xa0, 0x7e,
                    0xd5, 0xc0, 0xde,
                    0xff, 0x61, 0x60,
                };

                Array.Copy(generatedBytes, bytes, 9);
            });

        Assert.Equal("0aB-1cDe_2Fg", this.randomTokenGenerator.Generate(3));
    }

    #endregion
}
