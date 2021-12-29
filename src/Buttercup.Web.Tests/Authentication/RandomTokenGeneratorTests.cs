using System.Security.Cryptography;
using Moq;
using Xunit;

namespace Buttercup.Web.Authentication
{
    public class RandomTokenGeneratorTests
    {
        #region Generate

        [Theory]
        [InlineData(1)]
        [InlineData(3)]
        public void GenerateUses3nRandomBytes(int n)
        {
            var fixture = new RandomTokenGeneratorFixture();

            fixture.RandomTokenGenerator.Generate(n);

            fixture.MockRandomNumberGenerator
                .Verify(x => x.GetBytes(It.Is<byte[]>(bytes => bytes.Length == (3 * n))));
        }

        [Fact]
        public void GenerateReturnsUrlSafeBase64()
        {
            var fixture = new RandomTokenGeneratorFixture();

            fixture.MockRandomNumberGenerator
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

            Assert.Equal("0aB-1cDe_2Fg", fixture.RandomTokenGenerator.Generate(3));
        }

        #endregion

        private class RandomTokenGeneratorFixture
        {
            public RandomTokenGeneratorFixture()
            {
                var randomNumberGeneratorFactory = Mock.Of<IRandomNumberGeneratorFactory>(
                    x => x.Create() == this.MockRandomNumberGenerator.Object);

                this.RandomTokenGenerator = new(randomNumberGeneratorFactory);
            }

            public RandomTokenGenerator RandomTokenGenerator { get; }

            public Mock<RandomNumberGenerator> MockRandomNumberGenerator { get; } = new();
        }
    }
}
