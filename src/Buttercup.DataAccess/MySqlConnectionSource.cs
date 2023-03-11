using Microsoft.Extensions.Options;
using MySqlConnector;

namespace Buttercup.DataAccess;

internal class MySqlConnectionSource : IMySqlConnectionSource
{
    private readonly string connectionString;

    public MySqlConnectionSource(IOptions<DataAccessOptions> optionsAccessor) =>
        this.connectionString = new MySqlConnectionStringBuilder(
            optionsAccessor.Value.ConnectionString)
        {
            DateTimeKind = MySqlDateTimeKind.Utc,
        }.ToString();

    public async Task<MySqlConnection> OpenConnection()
    {
        var connection = new MySqlConnection(this.connectionString);

        await connection.OpenAsync();

        return connection;
    }
}
