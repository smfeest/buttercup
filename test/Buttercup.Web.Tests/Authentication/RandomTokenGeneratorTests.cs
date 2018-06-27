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

        [Fact]
        public void GenerateReturns36BytesAsUrlSafeBase64()
        {
            var mockRandomNumberGeneratorFactory = new MockRandomNumberGeneratorFactory();

            mockRandomNumberGeneratorFactory
                .MockRandomNumberGenerator
                .Setup(x => x.GetBytes(It.Is<byte[]>(bytes => bytes.Length == 36)))
                .Callback((byte[] bytes) =>
                {
#pragma warning disable SA1117
                    var generatedBytes = new byte[]
                    {
                        0x00, 0x10, 0x83, 0x10, 0x51, 0x87,
                        0x20, 0x92, 0x8b, 0x30, 0xd3, 0x8f,
                        0x41, 0x14, 0x93, 0x51, 0x55, 0x97,
                        0x61, 0x9f, 0xb4, 0xd7, 0x6d, 0xf8,
                        0xe7, 0xae, 0xfc, 0xf7, 0xf6, 0x9b,
                        0x71, 0xd7, 0x9f, 0x82, 0x18, 0xa3,
                    };
#pragma warning restore SA1117

                    Array.Copy(generatedBytes, bytes, 36);
                });

            var randomTokenGenerator = new RandomTokenGenerator(mockRandomNumberGeneratorFactory);

            Assert.Equal(
                "ABCDEFGHIJKLMNOPQRSTUVWXYZ-0123456789_abcdefghij",
                randomTokenGenerator.Generate());
        }

        #endregion

        private class MockRandomNumberGeneratorFactory : IRandomNumberGeneratorFactory
        {
            public Mock<RandomNumberGenerator> MockRandomNumberGenerator { get; } =
                new Mock<RandomNumberGenerator>();

            public RandomNumberGenerator Create() => this.MockRandomNumberGenerator.Object;
        }
    }
}
