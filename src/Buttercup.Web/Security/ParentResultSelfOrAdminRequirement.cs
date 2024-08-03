using Buttercup.EntityModel;
using Buttercup.Security;
using Buttercup.Web.Api;
using HotChocolate.Resolvers;
using Microsoft.AspNetCore.Authorization;

namespace Buttercup.Web.Security;

/// <summary>
/// An <see cref="IAuthorizationRequirement"/> that is satisfied if either the resource is an <see
/// cref="IMiddlewareContext"/> where the parent result represents the current user, or the current
/// user has the <see cref="RoleNames.Admin"/> role.
/// </summary>
public sealed class ParentResultSelfOrAdminRequirement :
    IAuthorizationHandler, IAuthorizationRequirement
{
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        if (context.User.IsInRole(RoleNames.Admin) || ParentObjectIsSelf(context))
        {
            context.Succeed(this);
        }

        return Task.CompletedTask;
    }

    private static bool ParentObjectIsSelf(AuthorizationHandlerContext context) =>
        context.Resource is IMiddlewareContext { ObjectType: UserType } middlewareContext &&
        middlewareContext.Parent<User>() is var parentUser &&
        context.User.HasUserId(parentUser.Id);
}
