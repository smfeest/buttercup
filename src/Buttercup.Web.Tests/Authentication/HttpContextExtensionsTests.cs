using Buttercup.Models;
using Buttercup.TestUtils;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Buttercup.Web.Authentication;

public class HttpContextExtensionsTests
{
    #region GetCurrentUser

    [Fact]
    public void GetCurrentUserReturnsUserFromItems()
    {
        var user = ModelFactory.CreateUser();

        var httpContext = new DefaultHttpContext();

        httpContext.Items.Add(typeof(User), user);

        Assert.Equal(user, httpContext.GetCurrentUser());
    }

    [Fact]
    public void GetCurrentUserReturnsNullIfNoUserInItems()
    {
        var httpContext = new DefaultHttpContext();

        Assert.Null(httpContext.GetCurrentUser());
    }

    #endregion

    #region SetCurrentUser

    [Fact]
    public void SetCurrentUserAddsUserToItems()
    {
        var user = ModelFactory.CreateUser();

        var httpContext = new DefaultHttpContext();

        httpContext.SetCurrentUser(user);

        Assert.Equal(user, httpContext.Items[typeof(User)]);
    }

    #endregion
}
