using Buttercup.EntityModel;
using Buttercup.Security;
using Microsoft.AspNetCore.Authorization;

namespace Buttercup.Web.Security;

/// <summary>
/// An <see cref="IAuthorizationRequirement"/> that is satisfied if either the resource represents a
/// comment authored by the current user, or the current user has the <see cref="RoleNames.Admin"/>
/// role.
/// </summary>
public sealed class CommentAuthorOrAdminRequirement
    : IAuthorizationHandler, IAuthorizationRequirement
{
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        if (context.User.IsInRole(RoleNames.Admin) || IsAuthor(context))
        {
            context.Succeed(this);
        }

        return Task.CompletedTask;
    }

    private static bool IsAuthor(AuthorizationHandlerContext context) =>
        context.Resource is Comment { AuthorId: var authorId } &&
            authorId.HasValue &&
            context.User.HasUserId(authorId.Value);
}
