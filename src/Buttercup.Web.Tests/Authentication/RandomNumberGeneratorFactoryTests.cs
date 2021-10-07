using Xunit;

namespace Buttercup.Web.Authentication
{
    public class RandomNumberGeneratorFactoryTests
    {
        #region Create

        [Fact]
        public void CreateReturnsNewRandomNumberGenerator()
        {
            var randomNumberGeneratorFactory = new RandomNumberGeneratorFactory();

            Assert.NotSame(
                randomNumberGeneratorFactory.Create(),
                randomNumberGeneratorFactory.Create());
        }

        #endregion
    }
}