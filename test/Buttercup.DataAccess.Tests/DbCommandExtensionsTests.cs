using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
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

        #region AddParameterWithStringValue

        [Fact]
        public void AddParameterWithStringValueSetsName()
        {
            var context = new AddParameterContext();

            context.MockDbCommand.Object.AddParameterWithStringValue("@alpha", "beta");

            context.MockDbParameter.VerifySet(p => p.ParameterName = "@alpha");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("\n")]
        public void AddParameterWithStringValueConvertsNullAndWhiteSpaceValues(string value)
        {
            var context = new AddParameterContext();

            context.MockDbCommand.Object.AddParameterWithStringValue("@alpha", value);

            context.MockDbParameter.VerifySet(p => p.Value = DBNull.Value);
        }

        [Fact]
        public void AddParameterWithStringValueTrimsValue()
        {
            var context = new AddParameterContext();

            context.MockDbCommand.Object.AddParameterWithStringValue("@alpha", "  beta\t  ");

            context.MockDbParameter.VerifySet(p => p.Value = "beta");
        }

        [Fact]
        public void AddParameterWithStringValueAddsParameter()
        {
            var context = new AddParameterContext();

            context.MockDbCommand.Object.AddParameterWithStringValue("@alpha", "beta");

            context.MockDbParameterCollection.Verify(c => c.Add(context.MockDbParameter.Object));
        }

        [Fact]
        public void AddParameterWithStringValueReturnsParameter()
        {
            var context = new AddParameterContext();

            var parameter = context.MockDbCommand.Object.AddParameterWithStringValue(
                "@alpha", "beta");

            Assert.Same(context.MockDbParameter.Object, parameter);
        }

        #endregion

        #region ExecuteScalarAsync

        [Fact]
        public async Task ExecuteScalarAsyncReturnsValue()
        {
            var command = MockCommandWithScalarResult(54);

            Assert.Equal(54, await command.ExecuteScalarAsync<int>());
            Assert.Equal(54, await command.ExecuteScalarAsync<int?>());
        }

        [Fact]
        public async Task ExecuteScalarAsyncReturnsDefaultValueWhenColumnContainsNull()
        {
            var command = MockCommandWithScalarResult(DBNull.Value);

            Assert.Equal(0, await command.ExecuteScalarAsync<long>());
            Assert.Null(await command.ExecuteScalarAsync<long?>());
            Assert.Null(await command.ExecuteScalarAsync<string>());
        }

        [Fact]
        public async Task ExecuteScalarAsyncReturnsDefaultValueWhenResultSetIsEmpty()
        {
            var command = MockCommandWithScalarResult(null);

            Assert.Equal(default(DateTime), await command.ExecuteScalarAsync<DateTime>());
            Assert.Null(await command.ExecuteScalarAsync<DateTime?>());
            Assert.Null(await command.ExecuteScalarAsync<string>());
        }

        private static DbCommand MockCommandWithScalarResult(object result) =>
            Mock.Of<DbCommand>(x => x.ExecuteScalarAsync(It.IsAny<CancellationToken>()) ==
                Task.FromResult(result));

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

            public Mock<DbCommand> MockDbCommand { get; } = new();

            public Mock<DbParameter> MockDbParameter { get; } = new();

            public Mock<DbParameterCollection> MockDbParameterCollection { get; } = new();
        }
    }
}
