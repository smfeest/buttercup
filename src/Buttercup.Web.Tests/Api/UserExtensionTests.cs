using Buttercup.DataAccess;
using Buttercup.EntityModel;
using Buttercup.TestUtils;
using Moq;
using Xunit;

namespace Buttercup.Web.Api;

public class UserExtensionTests
{
    private readonly ModelFactory modelFactory = new();

    #region GetUsersByIdAsync

    [Fact]
    public async void GetUsersByIdAsyncFetchesUsersById()
    {
        using var dbContextFactory = new FakeDbContextFactory();

        IList<User> users = new[]
        {
            this.modelFactory.BuildUser(),
            this.modelFactory.BuildUser()
        };

        var userIds = users.Select(user => user.Id).ToArray();

        var userDataProvider = Mock.Of<IUserDataProvider>(
            x => x.GetUsers(dbContextFactory.FakeDbContext, userIds) == Task.FromResult(users));

        var result = await UserExtension.GetUsersByIdAsync(
            userIds, dbContextFactory, userDataProvider);

        Assert.Equal(result[userIds[0]], users[0]);
        Assert.Equal(result[userIds[1]], users[1]);
    }

    #endregion
}
