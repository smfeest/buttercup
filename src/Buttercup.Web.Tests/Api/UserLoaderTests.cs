using Buttercup.DataAccess;
using Buttercup.Models;
using Buttercup.TestUtils;
using GreenDonut;
using Moq;
using MySqlConnector;
using Xunit;

namespace Buttercup.Web.Api;

public class UserLoaderTests
{
    [Fact]
    public async void FetchesAndCachesUsersById()
    {
        var mySqlConnection = new MySqlConnection();
        var mySqlConnectionSource = Mock.Of<IMySqlConnectionSource>(
            x => x.OpenConnection() == Task.FromResult(mySqlConnection));

        IList<User> users = new[] { ModelFactory.CreateUser(), ModelFactory.CreateUser() };
        var userIds = new[] { users[0].Id, users[1].Id };

        var userDataProvider = Mock.Of<IUserDataProvider>(
            x => x.GetUsers(mySqlConnection, userIds) == Task.FromResult(users),
            MockBehavior.Strict);

        using var userLoader = new UserLoader(
            mySqlConnectionSource, userDataProvider, new AutoBatchScheduler(), null);

        Assert.Equal(users, await userLoader.LoadAsync(new[] { users[0].Id, users[1].Id }));
        Assert.Equal(users[0], await userLoader.LoadAsync(users[0].Id));
    }
}
