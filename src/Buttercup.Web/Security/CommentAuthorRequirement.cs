using Buttercup.EntityModel;
using Buttercup.Security;
using Microsoft.AspNetCore.Authorization;

namespace Buttercup.Web.Security;

/// <summary>
/// An <see cref="IAuthorizationRequirement"/> that is satisfied if the resource represents a
/// comment authored by the current user.
/// </summary>
public sealed class CommentAuthorRequirement : IAuthorizationHandler, IAuthorizationRequirement
{
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        if (context.Resource is Comment { AuthorId: var authorId } &&
            authorId.HasValue &&
            context.User.HasUserId(authorId.Value))
        {
            context.Succeed(this);
        }

        return Task.CompletedTask;
    }
}
