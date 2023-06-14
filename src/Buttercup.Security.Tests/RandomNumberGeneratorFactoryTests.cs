using System.Security.Cryptography;
using Xunit;

namespace Buttercup.Security;

public sealed class RandomNumberGeneratorFactoryTests
{
    #region Create

    [Fact]
    public void CreateReturnsRandomNumberGenerator() =>
        Assert.IsAssignableFrom<RandomNumberGenerator>(new RandomNumberGeneratorFactory().Create());

    #endregion
}
