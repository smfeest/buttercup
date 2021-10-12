using System.Data;
using System.Threading.Tasks;
using Xunit;

namespace Buttercup.DataAccess
{
    [Collection("Database collection")]
    public class DbConnectionSourceTests
    {
        [Fact]
        public async Task ReturnsOpenConnectionToDatabase()
        {
            var connectionString = TestDatabase.BuildConnectionString();

            var connectionSource = new DbConnectionSource(connectionString);

            var connection = await connectionSource.OpenConnection();

            Assert.Equal(connectionString, connection.ConnectionString);
            Assert.Equal(ConnectionState.Open, connection.State);
        }
    }
}
