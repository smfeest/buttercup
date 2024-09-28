using Buttercup.Security;
using Buttercup.Web.Api;
using HotChocolate.Data.Sorting;
using HotChocolate.Language;
using HotChocolate.Resolvers;
using Microsoft.AspNetCore.Authorization;

namespace Buttercup.Web.Security;

/// <summary>
/// An <see cref="IAuthorizationRequirement"/> that is satisfied unless an admin-only sort field has
/// been used and the current user does not have the <see cref="RoleNames.Admin"/> role.
/// </summary>
/// <remarks>
/// Authorization will also fail if the <see cref="AuthorizationHandlerContext.Resource"/> is not an
/// <see cref="IMiddlewareContext"/>.
/// </remarks>
public sealed class AdminOnlySortFieldsRequirement
    : IAuthorizationHandler, IAuthorizationRequirement
{
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        if (context.Resource is IMiddlewareContext middlewareContext &&
            (
                context.User.IsInRole(RoleNames.Admin) ||
                !OrderArgumentContainsAdminOnlySortField(middlewareContext)
            ))
        {
            context.Succeed(this);
        }

        return Task.CompletedTask;
    }

    private static bool OrderArgumentContainsAdminOnlySortField(
        IMiddlewareContext middlewareContext) =>
        middlewareContext.Selection.Arguments.TryGetValue("order", out var argValue) &&
            NodeContainsAdminOnlySortField(argValue.Type, argValue.ValueLiteral);

    private static bool NodeContainsAdminOnlySortField(IType inputType, IValueNode? valueLiteral) =>
        valueLiteral switch
        {
            ListValueNode listValueNode =>
                ListNodeContainsAdminOnlySortField(listValueNode, inputType.ElementType()),
            ObjectValueNode objectValueNode =>
                ObjectNodeContainsAdminOnlySortField(objectValueNode, inputType.NamedType()),
            _ => false,
        };

    public static bool ListNodeContainsAdminOnlySortField(
        ListValueNode listValueNode, IType elementType) =>
        listValueNode.Items.Any(itemNode => NodeContainsAdminOnlySortField(elementType, itemNode));

    public static bool ObjectNodeContainsAdminOnlySortField(
        ObjectValueNode objectValueNode, INamedType namedType) =>
        namedType is SortInputType inputObjectType && objectValueNode.Fields.Any(fieldNode =>
        {
            var field = inputObjectType.Fields[fieldNode.Name.Value];
            return field.Directives.ContainsDirective(AdminOnlyDirectiveType.DirectiveName) ||
                NodeContainsAdminOnlySortField(field.Type, fieldNode.Value);
        });
}
