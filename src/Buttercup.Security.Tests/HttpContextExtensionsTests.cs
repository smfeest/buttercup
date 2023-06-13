using Buttercup.EntityModel;
using Buttercup.TestUtils;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace Buttercup.Security;

public sealed class HttpContextExtensionsTests
{
    private readonly ModelFactory modelFactory = new();

    #region GetCurrentUser

    [Fact]
    public void GetCurrentUserReturnsUserFromItems()
    {
        var user = this.modelFactory.BuildUser();

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
        var user = this.modelFactory.BuildUser();

        var httpContext = new DefaultHttpContext();

        httpContext.SetCurrentUser(user);

        Assert.Equal(user, httpContext.Items[typeof(User)]);
    }

    #endregion
}
