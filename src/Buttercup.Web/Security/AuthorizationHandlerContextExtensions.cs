using HotChocolate.Resolvers;
using Microsoft.AspNetCore.Authorization;

namespace Buttercup.Web.Security;

/// <summary>
/// Provides extension methods for <see cref="AuthorizationHandlerContext" />.
/// </summary>
public static class AuthorizationHandlerContextExtensions
{
    /// <summary>
    /// Returns the <see cref="IMiddlewareContext.Result"/> if the <see
    /// cref="AuthorizationHandlerContext.Resource"/> is an <see cref="IMiddlewareContext"/>, or
    /// <see cref="AuthorizationHandlerContext.Resource"/> otherwise.
    /// </summary>
    /// <param name="context">The authorization context.</param>
    /// <returns>The unwrapped resource.</returns>
    public static object? UnwrapResource(this AuthorizationHandlerContext context) =>
        context.Resource is IMiddlewareContext middlewareContext ?
            middlewareContext.Result :
            context.Resource;
}
