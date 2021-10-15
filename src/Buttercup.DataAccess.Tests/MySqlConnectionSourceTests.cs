using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MySqlConnector;
using Xunit;

namespace Buttercup.DataAccess
{
    [Collection("Database collection")]
    public class MySqlConnectionSourceTests
    {
        private static IOptions<DataAccessOptions> OptionsAccessor { get; } =
            Options.Create(
                new DataAccessOptions { ConnectionString = TestDatabase.BuildConnectionString() });

        [Fact]
        public async Task ReturnsOpenConnection()
        {
            var connectionSource = new MySqlConnectionSource(OptionsAccessor);

            using var connection = await connectionSource.OpenConnection();

            Assert.Equal(ConnectionState.Open, connection.State);
        }

        [Fact]
        public async Task SetsDateTimeKindToUtc()
        {
            var connectionSource = new MySqlConnectionSource(OptionsAccessor);

            using var connection = await connectionSource.OpenConnection();

            var connectionStringBuilder = new MySqlConnectionStringBuilder(
                connection.ConnectionString);

            Assert.Equal(MySqlDateTimeKind.Utc, connectionStringBuilder.DateTimeKind);
        }
    }
}
