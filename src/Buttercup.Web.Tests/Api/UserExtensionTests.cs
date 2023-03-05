using Buttercup.DataAccess;
using Buttercup.Models;
using Buttercup.TestUtils;
using Moq;
using MySqlConnector;
using Xunit;

namespace Buttercup.Web.Api;

public class UserExtensionTests
{
    #region GetUsersByIdAsync

    [Fact]
    public async void GetUsersByIdAsyncFetchesUsersById()
    {
        using var mySqlConnection = new MySqlConnection();
        var mySqlConnectionSource = Mock.Of<IMySqlConnectionSource>(
            x => x.OpenConnection() == Task.FromResult(mySqlConnection));

        IList<User> users = new[] { ModelFactory.CreateUser(), ModelFactory.CreateUser() };
        var userIds = users.Select(user => user.Id).ToArray();

        var userDataProvider = Mock.Of<IUserDataProvider>(
            x => x.GetUsers(mySqlConnection, userIds) == Task.FromResult(users));

        var result = await UserExtension.GetUsersByIdAsync(
            userIds, mySqlConnectionSource, userDataProvider);

        Assert.Equal(result[userIds[0]], users[0]);
        Assert.Equal(result[userIds[1]], users[1]);
    }

    #endregion
}
