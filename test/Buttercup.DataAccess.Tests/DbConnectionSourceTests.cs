using System.Data;
using System.Threading.Tasks;
using Xunit;

namespace Buttercup.DataAccess
{
    [Collection("Database collection")]
    public class DbConnectionSourceTests
    {
        private readonly DatabaseFixture databaseFixture;

        public DbConnectionSourceTests(DatabaseFixture databaseFixture) =>
            this.databaseFixture = databaseFixture;

        [Fact]
        public async Task ReturnsOpenConnectionToDatabase()
        {
            var connectionString = this.databaseFixture.DatabaseConnectionString;

            var connectionSource = new DbConnectionSource(connectionString);

            var connection = await connectionSource.OpenConnection();

            Assert.Equal(connectionString, connection.ConnectionString);
            Assert.Equal(ConnectionState.Open, connection.State);
        }
    }
}
