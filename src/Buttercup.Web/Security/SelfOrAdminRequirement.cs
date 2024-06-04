using Buttercup.EntityModel;
using Buttercup.Security;
using Buttercup.Web.Api;
using HotChocolate.Resolvers;
using Microsoft.AspNetCore.Authorization;

namespace Buttercup.Web.Security;

/// <summary>
/// An <see cref="IAuthorizationRequirement"/> that is satisfied if either the resource represents
/// the current user, or the current user has the <see cref="RoleNames.Admin"/> role.
/// </summary>
public sealed class SelfOrAdminRequirement : IAuthorizationHandler, IAuthorizationRequirement
{
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        if (context.User.IsInRole(RoleNames.Admin) || IsSelf(context))
        {
            context.Succeed(this);
        }

        return Task.CompletedTask;
    }

    private static bool IsSelf(AuthorizationHandlerContext context)
    {
        var userResource = context.Resource switch
        {
            User user => user,
            IMiddlewareContext { ObjectType: UserType } middlewareContext =>
                middlewareContext.Parent<User>(),
            _ => null,
        };

        return userResource is not null && context.User.HasUserId(userResource.Id);
    }
}
