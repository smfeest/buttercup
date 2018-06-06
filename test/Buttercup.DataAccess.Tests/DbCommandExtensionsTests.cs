using System;
using System.Data;
using System.Data.Common;
using Moq;
using Moq.Protected;
using Xunit;

namespace Buttercup.DataAccess
{
    public class DbCommandExtensionsTests
    {
        #region AddParameterWithValue

        [Fact]
        public void AddParameterWithValueSetsName()
        {
            var context = new AddParameterContext();

            context.MockDbCommand.Object.AddParameterWithValue("@alpha", "beta");

            context.MockDbParameter.VerifySet(p => p.ParameterName = "@alpha");
        }

        [Theory]
        [InlineData("")]
        [InlineData("beta")]
        [InlineData(339)]
        public void AddParameterWithValueSetsValue(object value)
        {
            var context = new AddParameterContext();

            context.MockDbCommand.Object.AddParameterWithValue("@alpha", value);

            context.MockDbParameter.VerifySet(p => p.Value = value);
        }

        [Fact]
        public void AddParameterWithValueConvertsNullValue()
        {
            var context = new AddParameterContext();

            context.MockDbCommand.Object.AddParameterWithValue("@alpha", null);

            context.MockDbParameter.VerifySet(p => p.Value = DBNull.Value);
        }

        [Fact]
        public void AddParameterWithValueAddsParameter()
        {
            var context = new AddParameterContext();

            context.MockDbCommand.Object.AddParameterWithValue("@alpha", "beta");

            context.MockDbParameterCollection.Verify(c => c.Add(context.MockDbParameter.Object));
        }

        [Fact]
        public void AddParameterWithValueReturnsParameter()
        {
            var context = new AddParameterContext();

            var parameter = context.MockDbCommand.Object.AddParameterWithValue("@alpha", "beta");

            Assert.Same(context.MockDbParameter.Object, parameter);
        }

        #endregion

        private class AddParameterContext
        {
            public AddParameterContext()
            {
                this.MockDbCommand.Protected()
                    .Setup<DbParameter>("CreateDbParameter")
                    .Returns(this.MockDbParameter.Object);

                this.MockDbCommand.Protected()
                    .SetupGet<DbParameterCollection>("DbParameterCollection")
                    .Returns(this.MockDbParameterCollection.Object);
            }

            public Mock<DbCommand> MockDbCommand { get; } = new Mock<DbCommand>();

            public Mock<DbParameter> MockDbParameter { get; } = new Mock<DbParameter>();

            public Mock<DbParameterCollection> MockDbParameterCollection { get; } =
                new Mock<DbParameterCollection>();
        }
    }
}
