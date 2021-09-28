using System;
using System.Diagnostics.CodeAnalysis;
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
            var context = new Context();

            context.RandomTokenGenerator.Generate(n);

            context
                .MockRandomNumberGeneratorFactory
                .MockRandomNumberGenerator
                .Verify(x => x.GetBytes(It.Is<byte[]>(bytes => bytes.Length == (3 * n))));
        }

        [Fact]
        public void GenerateReturnsUrlSafeBase64()
        {
            var context = new Context();

            context.MockRandomNumberGeneratorFactory
                .MockRandomNumberGenerator
                .Setup(x => x.GetBytes(It.IsAny<byte[]>()))
                .Callback((byte[] bytes) =>
                {
#pragma warning disable SA1117
                    var generatedBytes = new byte[]
                    {
                        0xd1, 0xa0, 0x7e,
                        0xd5, 0xc0, 0xde,
                        0xff, 0x61, 0x60,
                    };
#pragma warning restore SA1117

                    Array.Copy(generatedBytes, bytes, 9);
                });

            Assert.Equal("0aB-1cDe_2Fg", context.RandomTokenGenerator.Generate(3));
        }

        #endregion

        private class Context
        {
            public Context()
            {
                this.RandomTokenGenerator = new(this.MockRandomNumberGeneratorFactory);
            }

            public RandomTokenGenerator RandomTokenGenerator { get; }

            public MockRandomNumberGeneratorFactory MockRandomNumberGeneratorFactory { get; } =
                new();
        }

        private class MockRandomNumberGeneratorFactory : IRandomNumberGeneratorFactory
        {
            public Mock<RandomNumberGenerator> MockRandomNumberGenerator { get; } = new();

            public RandomNumberGenerator Create() => this.MockRandomNumberGenerator.Object;
        }
    }
}
