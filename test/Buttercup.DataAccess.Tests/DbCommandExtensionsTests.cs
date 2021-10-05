using System;
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
            var fixture = new AddParameterFixture();

            fixture.MockDbCommand.Object.AddParameterWithValue("@alpha", "beta");

            fixture.MockDbParameter.VerifySet(p => p.ParameterName = "@alpha");
        }

        [Theory]
        [InlineData("")]
        [InlineData("beta")]
        [InlineData(339)]
        public void AddParameterWithValueSetsValue(object value)
        {
            var fixture = new AddParameterFixture();

            fixture.MockDbCommand.Object.AddParameterWithValue("@alpha", value);

            fixture.MockDbParameter.VerifySet(p => p.Value = value);
        }

        [Fact]
        public void AddParameterWithValueConvertsNullValue()
        {
            var fixture = new AddParameterFixture();

            fixture.MockDbCommand.Object.AddParameterWithValue("@alpha", null);

            fixture.MockDbParameter.VerifySet(p => p.Value = DBNull.Value);
        }

        [Fact]
        public void AddParameterWithValueAddsParameter()
        {
            var fixture = new AddParameterFixture();

            fixture.MockDbCommand.Object.AddParameterWithValue("@alpha", "beta");

            fixture.MockDbParameterCollection.Verify(c => c.Add(fixture.MockDbParameter.Object));
        }

        [Fact]
        public void AddParameterWithValueReturnsParameter()
        {
            var fixture = new AddParameterFixture();

            var parameter = fixture.MockDbCommand.Object.AddParameterWithValue("@alpha", "beta");

            Assert.Same(fixture.MockDbParameter.Object, parameter);
        }

        #endregion

        #region AddParameterWithStringValue

        [Fact]
        public void AddParameterWithStringValueSetsName()
        {
            var fixture = new AddParameterFixture();

            fixture.MockDbCommand.Object.AddParameterWithStringValue("@alpha", "beta");

            fixture.MockDbParameter.VerifySet(p => p.ParameterName = "@alpha");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("\n")]
        public void AddParameterWithStringValueConvertsNullAndWhiteSpaceValues(string value)
        {
            var fixture = new AddParameterFixture();

            fixture.MockDbCommand.Object.AddParameterWithStringValue("@alpha", value);

            fixture.MockDbParameter.VerifySet(p => p.Value = DBNull.Value);
        }

        [Fact]
        public void AddParameterWithStringValueTrimsValue()
        {
            var fixture = new AddParameterFixture();

            fixture.MockDbCommand.Object.AddParameterWithStringValue("@alpha", "  beta\t  ");

            fixture.MockDbParameter.VerifySet(p => p.Value = "beta");
        }

        [Fact]
        public void AddParameterWithStringValueAddsParameter()
        {
            var fixture = new AddParameterFixture();

            fixture.MockDbCommand.Object.AddParameterWithStringValue("@alpha", "beta");

            fixture.MockDbParameterCollection.Verify(c => c.Add(fixture.MockDbParameter.Object));
        }

        [Fact]
        public void AddParameterWithStringValueReturnsParameter()
        {
            var fixture = new AddParameterFixture();

            var parameter = fixture.MockDbCommand.Object.AddParameterWithStringValue(
                "@alpha", "beta");

            Assert.Same(fixture.MockDbParameter.Object, parameter);
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

            Assert.Equal(default, await command.ExecuteScalarAsync<DateTime>());
            Assert.Null(await command.ExecuteScalarAsync<DateTime?>());
            Assert.Null(await command.ExecuteScalarAsync<string>());
        }

        private static DbCommand MockCommandWithScalarResult(object? result) =>
            Mock.Of<DbCommand>(x => x.ExecuteScalarAsync(It.IsAny<CancellationToken>()) ==
                Task.FromResult(result));

        #endregion

        private class AddParameterFixture
        {
            public AddParameterFixture()
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
