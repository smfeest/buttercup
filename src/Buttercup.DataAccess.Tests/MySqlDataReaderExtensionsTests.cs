using System;
using System.Threading.Tasks;
using Xunit;

namespace Buttercup.DataAccess
{
    [Collection("Database collection")]
    public class MySqlDataReaderExtensionsTests
    {
        #region GetDateTime

        [Theory]
        [InlineData(DateTimeKind.Local)]
        [InlineData(DateTimeKind.Unspecified)]
        [InlineData(DateTimeKind.Utc)]
        public async Task GetDateTimeReturnsValueWithDateTimeKind(DateTimeKind kind)
        {
            using var connection = await TestDatabase.OpenConnectionWithRollback();

            using var command = connection.CreateCommand();

            command.CommandText = "SELECT '2000-01-02 03:04:05' column_name";

            using var reader = await command.ExecuteReaderAsync();

            await reader.ReadAsync();

            var result = reader.GetDateTime("column_name", kind);

            Assert.Equal(new(2000, 1, 2, 3, 4, 5), result);
            Assert.Equal(kind, result.Kind);
        }

        #endregion

        #region GetNullableDateTime

        [Theory]
        [InlineData(DateTimeKind.Local)]
        [InlineData(DateTimeKind.Unspecified)]
        [InlineData(DateTimeKind.Utc)]
        public async Task GetNullableDateTimeReturnsValueWhenNotDbNull(DateTimeKind kind)
        {
            using var connection = await TestDatabase.OpenConnectionWithRollback();

            using var command = connection.CreateCommand();

            command.CommandText = "SELECT '2000-01-02 03:04:05' column_name";

            using var reader = await command.ExecuteReaderAsync();

            await reader.ReadAsync();

            var result = reader.GetNullableDateTime("column_name", kind);

            Assert.Equal(new(2000, 1, 2, 3, 4, 5), result);
            Assert.Equal(kind, result!.Value.Kind);
        }

        [Fact]
        public async Task GetNullableDateTimeReturnsNullWhenValueIsDbNull()
        {
            using var connection = await TestDatabase.OpenConnectionWithRollback();

            using var command = connection.CreateCommand();

            command.CommandText = "SELECT NULL column_name";

            using var reader = await command.ExecuteReaderAsync();

            await reader.ReadAsync();

            Assert.Null(reader.GetNullableInt32("column_name"));
        }

        #endregion

        #region GetNullableInt32

        [Fact]
        public async Task GetNullableInt32ReturnsValueWhenNotDbNull()
        {
            using var connection = await TestDatabase.OpenConnectionWithRollback();

            using var command = connection.CreateCommand();

            command.CommandText = "SELECT 5 column_name";

            using var reader = await command.ExecuteReaderAsync();

            await reader.ReadAsync();

            Assert.Equal(5, reader.GetNullableInt32("column_name"));
        }

        [Fact]
        public async Task GetNullableInt32ReturnsNullWhenValueIsDbNull()
        {
            using var connection = await TestDatabase.OpenConnectionWithRollback();

            using var command = connection.CreateCommand();

            command.CommandText = "SELECT NULL column_name";

            using var reader = await command.ExecuteReaderAsync();

            await reader.ReadAsync();

            Assert.Null(reader.GetNullableInt32("column_name"));
        }

        #endregion

        #region GetNullableInt64

        [Fact]
        public async Task GetNullableInt64ReturnsValueWhenNotDbNull()
        {
            using var connection = await TestDatabase.OpenConnectionWithRollback();

            using var command = connection.CreateCommand();

            command.CommandText = "SELECT 1029384756 column_name";

            using var reader = await command.ExecuteReaderAsync();

            await reader.ReadAsync();

            Assert.Equal(1029384756, reader.GetNullableInt64("column_name"));
        }

        [Fact]
        public async Task GetNullableInt64ReturnsNullWhenValueIsDbNull()
        {
            using var connection = await TestDatabase.OpenConnectionWithRollback();

            using var command = connection.CreateCommand();

            command.CommandText = "SELECT NULL column_name";

            using var reader = await command.ExecuteReaderAsync();

            await reader.ReadAsync();

            Assert.Null(reader.GetNullableInt64("column_name"));
        }

        #endregion

        #region GetNullableString

        [Fact]
        public async Task GetNullableStringReturnsValueWhenNotDbNull()
        {
            using var connection = await TestDatabase.OpenConnectionWithRollback();

            using var command = connection.CreateCommand();

            command.CommandText = "SELECT 'string-value' column_name";

            using var reader = await command.ExecuteReaderAsync();

            await reader.ReadAsync();

            Assert.Equal("string-value", reader.GetNullableString("column_name"));
        }

        [Fact]
        public async Task GetNullableStringReturnsNullWhenValueIsDbNull()
        {
            using var connection = await TestDatabase.OpenConnectionWithRollback();

            using var command = connection.CreateCommand();

            command.CommandText = "SELECT NULL column_name";

            using var reader = await command.ExecuteReaderAsync();

            await reader.ReadAsync();

            Assert.Null(reader.GetNullableString("column_name"));
        }

        #endregion
    }
}
