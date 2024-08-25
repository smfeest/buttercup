using System.Security.Claims;
using HotChocolate.Language;
using HotChocolate.Resolvers;
using Microsoft.AspNetCore.Authorization;
using Moq;
using Xunit;

namespace Buttercup.Web.Security;

public sealed class RoleRestrictedOrderByFieldsRequirementTests
{
    private const string RequiredRole = "superhero";
    private const string OtherRole = "supervillain";

    [Fact]
    public async Task NoOrderArgument_NotInRequiredRole_IndicatesSuccess()
    {
        var context = await Handle(OtherRole, NullValueNode.Default);
        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task OrderArgumentWithoutRestrictedFields_NotInRequiredRole_IndicatesSuccess()
    {
        var context = await Handle(OtherRole, "[{ foo: ASC, quz: DESC }, { thud: ASC }]");
        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task OrderArgumentWithRestrictedFields_NotInRequiredRole_DoesNotIndicatesSuccess()
    {
        var context = await Handle(OtherRole, "[{ foo: ASC, bar: DESC }, { thud: ASC }]");
        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task OrderArgumentWithRestrictedFields_InRequiredRole_IndicatesSuccess()
    {
        var context = await Handle(RequiredRole, "[{ foo: ASC }, { thud: ASC, baz: DESC }]");
        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task ResourceIsNotMiddlewareContext_DoesNotIndicateSuccess()
    {
        var context = await Handle(RequiredRole, new object());
        Assert.False(context.HasSucceeded);
    }

    private static async Task<AuthorizationHandlerContext> Handle(string role, object resource)
    {
        var requirement = new RoleRestrictedOrderByFieldsRequirement(RequiredRole, "bar", "baz");
        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, role)]));
        var context = new AuthorizationHandlerContext([requirement], user, resource);

        await requirement.HandleAsync(context);

        return context;
    }

    private static Task<AuthorizationHandlerContext> Handle(
        string role, IValueNode orderArgumentLiteral)
    {
        var middlewareContext = Mock.Of<IMiddlewareContext>(
            x => x.ArgumentLiteral<IValueNode>("order") == orderArgumentLiteral);
        return Handle(role, middlewareContext);
    }

    private static Task<AuthorizationHandlerContext> Handle(string role, string orderArgument) =>
        Handle(role, Utf8GraphQLParser.Syntax.ParseValueLiteral(orderArgument));
}
