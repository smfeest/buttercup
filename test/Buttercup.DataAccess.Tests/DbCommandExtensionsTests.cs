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
            var mockCommand = new Mock<DbCommand>();

            mockCommand
                .Setup(x => x.ExecuteScalarAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(54);

            Assert.Equal(54, await mockCommand.Object.ExecuteScalarAsync<int>());
            Assert.Equal(54, await mockCommand.Object.ExecuteScalarAsync<int?>());
        }

        [Fact]
        public async Task ExecuteScalarAsyncReturnsDefaultValueWhenColumnContainsNull()
        {
            var mockCommand = new Mock<DbCommand>();

            mockCommand
                .Setup(x => x.ExecuteScalarAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(DBNull.Value);

            Assert.Equal(0, await mockCommand.Object.ExecuteScalarAsync<long>());
            Assert.Null(await mockCommand.Object.ExecuteScalarAsync<long?>());
            Assert.Null(await mockCommand.Object.ExecuteScalarAsync<string>());
        }

        [Fact]
        public async Task ExecuteScalarAsyncReturnsDefaultValueWhenResultSetIsEmpty()
        {
            var mockCommand = new Mock<DbCommand>();

            mockCommand
                .Setup(x => x.ExecuteScalarAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((object)null);

            Assert.Equal(default(DateTime), await mockCommand.Object.ExecuteScalarAsync<DateTime>());
            Assert.Null(await mockCommand.Object.ExecuteScalarAsync<DateTime?>());
            Assert.Null(await mockCommand.Object.ExecuteScalarAsync<string>());
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

            public Mock<DbCommand> MockDbCommand { get; } = new();

            public Mock<DbParameter> MockDbParameter { get; } = new();

            public Mock<DbParameterCollection> MockDbParameterCollection { get; } = new();
        }
    }
}
