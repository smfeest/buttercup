using System.Security.Claims;
using Buttercup.DataAccess;
using Buttercup.TestUtils;
using Moq;
using MySqlConnector;
using Xunit;

namespace Buttercup.Web.Api;

public class QueryTests
{
    private readonly Query query;
    private readonly MySqlConnection mySqlConnection = new();

    public QueryTests()
    {
        var mySqlConnectionSource = Mock.Of<IMySqlConnectionSource>(
            x => x.OpenConnection() == Task.FromResult(this.mySqlConnection));

        this.query = new(mySqlConnectionSource);
    }

    #region CurrentUser

    [Fact]
    public async Task CurrentUserReturnsCurrentUserWhenAuthenticated()
    {
        var user = ModelFactory.CreateUser();

        var userDataProvider = Mock.Of<IUserDataProvider>(
            x => x.GetUser(this.mySqlConnection, 1234) == Task.FromResult(user));

        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(new Claim[] { new(ClaimTypes.NameIdentifier, "1234") }));

        Assert.Equal(user, await this.query.CurrentUser(userDataProvider, principal));
    }

    [Fact]
    public async Task CurrentUserReturnsNullWhenNotAuthenticated() =>
        Assert.Null(
            await this.query.CurrentUser(Mock.Of<IUserDataProvider>(), new ClaimsPrincipal()));

    #endregion
}
