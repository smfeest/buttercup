using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Xunit;

namespace Buttercup.DataAccess
{
    [Collection("Database collection")]
    public class MySqlConnectionSourceTests
    {
        [Fact]
        public async Task ReturnsOpenConnectionToDatabase()
        {
            var connectionString = TestDatabase.BuildConnectionString();

            var connectionSource = new MySqlConnectionSource(
                Options.Create(new DataAccessOptions { ConnectionString = connectionString }));

            var connection = await connectionSource.OpenConnection();

            Assert.Equal(connectionString, connection.ConnectionString);
            Assert.Equal(ConnectionState.Open, connection.State);
        }
    }
}
