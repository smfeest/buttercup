using Buttercup.EntityModel;
using Buttercup.Security;
using Microsoft.AspNetCore.Authorization;

namespace Buttercup.Web.Security;

/// <summary>
/// An <see cref="IAuthorizationRequirement"/> that is satisfied if either the resource is not
/// soft-deleted, or the current user has the <see cref="RoleNames.Admin"/> role.
/// </summary>
public sealed class AdminWhenDeletedRequirement : IAuthorizationHandler, IAuthorizationRequirement
{
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        if (!IsDeleted(context) || context.User.IsInRole(RoleNames.Admin))
        {
            context.Succeed(this);
        }

        return Task.CompletedTask;
    }

    private static bool IsDeleted(AuthorizationHandlerContext context) =>
        context.UnwrapResource() is ISoftDeletable { Deleted: not null };
}
