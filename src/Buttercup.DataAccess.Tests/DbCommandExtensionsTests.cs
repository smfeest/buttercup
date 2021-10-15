using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace Buttercup.DataAccess
{
    public class DbCommandExtensionsTests
    {
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
    }
}
