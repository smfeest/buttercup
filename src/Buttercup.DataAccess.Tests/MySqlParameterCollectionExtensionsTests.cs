using MySqlConnector;
using Xunit;

namespace Buttercup.DataAccess
{
    public class MySqlParameterCollectionExtensionsTests
    {
        #region AddWithStringValue

        [Fact]
        public void AddWithStringValueAddsAndReturnsParameter()
        {
            using var command = new MySqlCommand();

            var parameter = command.Parameters.AddWithStringValue("@alpha", "beta");

            Assert.Same(command.Parameters["@alpha"], parameter);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("\n")]
        public void AddWithStringValueConvertsNullAndWhiteSpaceValues(string value)
        {
            using var command = new MySqlCommand();

            var parameter = command.Parameters.AddWithStringValue("@alpha", value);

            Assert.Null(parameter.Value);
        }

        [Fact]
        public void AddWithStringValueTrimsValue()
        {
            using var command = new MySqlCommand();

            var parameter = command.Parameters.AddWithStringValue("@alpha", "  beta\t  ");

            Assert.Equal("beta", parameter.Value);
        }

        #endregion
    }
}
