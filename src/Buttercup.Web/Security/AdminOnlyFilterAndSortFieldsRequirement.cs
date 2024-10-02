using Buttercup.Security;
using Buttercup.Web.Api;
using HotChocolate.Data.Filters;
using HotChocolate.Data.Sorting;
using HotChocolate.Language;
using HotChocolate.Resolvers;
using Microsoft.AspNetCore.Authorization;

namespace Buttercup.Web.Security;

/// <summary>
/// An <see cref="IAuthorizationRequirement"/> that is satisfied unless an admin-only filter or sort
/// field has been used and the current user does not have the <see cref="RoleNames.Admin"/> role.
/// </summary>
/// <remarks>
/// Authorization will also fail if the <see cref="AuthorizationHandlerContext.Resource"/> is not an
/// <see cref="IMiddlewareContext"/>.
/// </remarks>
public sealed class AdminOnlyFilterAndSortFieldsRequirement
    : IAuthorizationHandler, IAuthorizationRequirement
{
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        if (context.Resource is IMiddlewareContext middlewareContext &&
            (
                context.User.IsInRole(RoleNames.Admin) ||
                !ArgumentsContainAdminOnlyField(middlewareContext)
            ))
        {
            context.Succeed(this);
        }

        return Task.CompletedTask;
    }

    private static bool ArgumentsContainAdminOnlyField(IMiddlewareContext middlewareContext) =>
        ArgumentContainsAdminOnlyField<SortInputType>(middlewareContext, "order") ||
        ArgumentContainsAdminOnlyField<FilterInputType>(middlewareContext, "where");

    private static bool ArgumentContainsAdminOnlyField<T>(
        IMiddlewareContext middlewareContext, string argumentName) where T : InputObjectType =>
        middlewareContext.Selection.Arguments.TryGetValue(argumentName, out var argValue) &&
            NodeContainsAdminOnlyField<T>(argValue.Type, argValue.ValueLiteral);

    private static bool NodeContainsAdminOnlyField<T>(IType inputType, IValueNode? valueLiteral)
        where T : InputObjectType =>
        valueLiteral switch
        {
            ListValueNode listValueNode =>
                ListNodeContainsAdminOnlyField<T>(listValueNode, inputType.ElementType()),
            ObjectValueNode objectValueNode =>
                ObjectNodeContainsAdminOnlyField<T>(objectValueNode, inputType.NamedType()),
            _ => false,
        };

    public static bool ListNodeContainsAdminOnlyField<T>(
        ListValueNode listValueNode, IType elementType) where T : InputObjectType =>
        listValueNode.Items.Any(
            itemNode => NodeContainsAdminOnlyField<T>(elementType, itemNode));

    public static bool ObjectNodeContainsAdminOnlyField<T>(
        ObjectValueNode objectValueNode, INamedType namedType) where T : InputObjectType =>
        namedType is T inputObjectType && objectValueNode.Fields.Any(fieldNode =>
        {
            var field = inputObjectType.Fields[fieldNode.Name.Value];
            return field.Directives.ContainsDirective(AdminOnlyDirectiveType.DirectiveName) ||
                NodeContainsAdminOnlyField<T>(field.Type, fieldNode.Value);
        });
}
