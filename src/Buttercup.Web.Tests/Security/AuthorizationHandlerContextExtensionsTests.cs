using HotChocolate.Resolvers;
using Microsoft.AspNetCore.Authorization;
using Moq;
using Xunit;

namespace Buttercup.Web.Security;

public sealed class AuthorizationHandlerContextExtensionsTests
{
    #region UnwrapResource

    [Fact]
    public void UnwrapResource_ReturnsNullWhenResourceIsNull()
    {
        var context = new AuthorizationHandlerContext([], new(), null);
        Assert.Null(context.UnwrapResource());
    }

    [Fact]
    public void UnwrapResource_ReturnsResourceWhenResourceIsNotMiddlewareContext()
    {
        var resource = new object();
        var context = new AuthorizationHandlerContext([], new(), resource);
        Assert.Same(resource, context.UnwrapResource());
    }

    [Fact]
    public void UnwrapResource_ReturnsResultWhenResourceIsMiddlewareContext()
    {
        var result = new object();
        var middlewareContext = Mock.Of<IMiddlewareContext>(x => x.Result == result);
        var context = new AuthorizationHandlerContext([], new(), middlewareContext);
        Assert.Same(result, context.UnwrapResource());
    }

    #endregion
}
