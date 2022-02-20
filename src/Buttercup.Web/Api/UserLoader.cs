using Buttercup.DataAccess;
using Buttercup.Models;

namespace Buttercup.Web.Api;

public class UserLoader : BatchDataLoader<long, User>, IUserLoader
{
    private readonly IMySqlConnectionSource mySqlConnectionSource;
    private readonly IUserDataProvider userDataProvider;

    public UserLoader(
        IMySqlConnectionSource mySqlConnectionSource,
        IUserDataProvider userDataProvider,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        this.mySqlConnectionSource = mySqlConnectionSource;
        this.userDataProvider = userDataProvider;
    }

    protected override async Task<IReadOnlyDictionary<long, User>> LoadBatchAsync(
        IReadOnlyList<long> keys, CancellationToken cancellationToken)
    {
        using var connection = await this.mySqlConnectionSource.OpenConnection();

        var users = await this.userDataProvider.GetUsers(connection, keys);

        return users.ToDictionary(x => x.Id);
    }
}
