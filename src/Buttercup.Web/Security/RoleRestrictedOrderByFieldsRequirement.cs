using HotChocolate.Language;
using HotChocolate.Resolvers;
using Microsoft.AspNetCore.Authorization;

namespace Buttercup.Web.Security;

/// <summary>
/// An <see cref="IAuthorizationRequirement"/> that is satisfied unless an `order` argument has been
/// specified featuring any the specified restricted fields and the current user does not have the
/// specified role needed to order by those restricted fields.
/// </summary>
/// <remarks>
/// Authorization will also fail if the <see cref="AuthorizationHandlerContext.Resource"/> is not an
/// <see cref="IMiddlewareContext"/>.
/// </remarks>
/// <param name="requiredRole">
/// The role needed to order by any of the restricted fields.
/// </param>
/// <param name="restrictedFields">
/// The fields that cannot be used for ordering unless the user has the required role.
/// </param>
public sealed class RoleRestrictedOrderByFieldsRequirement(
    string requiredRole, params string[] restrictedFields)
    : IAuthorizationHandler, IAuthorizationRequirement
{
    public HashSet<string> RestrictedFields { get; } = new(restrictedFields);

    public string RequiredRole { get; } = requiredRole;

    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        if (context.Resource is IMiddlewareContext middlewareContext &&
            (
                context.User.IsInRole(this.RequiredRole) ||
                !this.HasOrderArgumentFeaturingRestrictedField(middlewareContext)
            ))
        {
            context.Succeed(this);
        }

        return Task.CompletedTask;
    }

    private bool HasOrderArgumentFeaturingRestrictedField(IMiddlewareContext middlewareContext) =>
        middlewareContext.ArgumentLiteral<IValueNode>("order") is ListValueNode listNode &&
            listNode.Items.Any(item =>
                item is ObjectValueNode objectNode &&
                objectNode.Fields.Any(field => this.RestrictedFields.Contains(field.Name.Value)));
}
