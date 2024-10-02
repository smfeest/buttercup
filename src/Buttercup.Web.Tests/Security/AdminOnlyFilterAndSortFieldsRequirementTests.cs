using System.Security.Claims;
using Buttercup.Security;
using Buttercup.Web.Api;
using HotChocolate;
using HotChocolate.Authorization;
using HotChocolate.Data;
using HotChocolate.Data.Filters;
using HotChocolate.Data.Sorting;
using HotChocolate.Execution;
using HotChocolate.Resolvers;
using Xunit;

namespace Buttercup.Web.Security;

public sealed class AdminOnlyFilterAndSortFieldsRequirementTests
{
    [Fact]
    public async Task NoOrderOrWhereArgumentWhenNotAnAdmin_IsAuthorized()
    {
        var result = await Execute("{ foos { field1 } }", isAdmin: false);
        Assert.Null(result.ExpectQueryResult().Errors);
    }

    [Theory]
    [InlineData("{ foos(order: { field1: ASC, bar1: { field4: DESC } }) { field1 } }")]
    [InlineData("""
        {
            foos(
                order: [{ field1: ASC, bar1: { field4: DESC } }, { field2: ASC }]
            ) { field1 }
        }
        """)]
    public async Task OrderArgumentWithoutAdminOnlyFieldsWhenNotAnAdmin_IsAuthorized(string query)
    {
        var result = await Execute(query, isAdmin: false);
        Assert.Null(result.ExpectQueryResult().Errors);
    }

    [Theory]
    [InlineData("{ foos(order: { field3: DESC }) { field1 } }")]
    [InlineData("{ foos(order: { field1: ASC, bar1: { field6: DESC } }) { field1 } }")]
    [InlineData("{ foos(order: { field1: ASC, bar2: { field4: DESC } }) { field1 } }")]
    [InlineData("""
        {
            foos(
                order: [{ field1: ASC, field2: DESC }, { field3: ASC }]
            ) { field1 }
        }
        """)]
    public async Task OrderArgumentWithAdminOnlyFieldsWhenNotAnAdmin_IsNotAuthorized(string query)
    {
        var result = await Execute(query, isAdmin: false);
        var errors = result.ExpectQueryResult().Errors;
        Assert.NotNull(errors);
        Assert.Equal(ErrorCodes.Authentication.NotAuthorized, Assert.Single(errors).Code);
    }

    [Fact]
    public async Task OrderArgumentWithAdminOnlyFieldsWhenAnAdmin_IsAuthorized()
    {
        var result = await Execute("""
            {
                foos(
                    order: {
                        field1: ASC,
                        field3: DESC,
                        bar1: { field6: DESC },
                        bar2: { field4: DESC }
                    }
                ) { field1 }
            }
            """,
            isAdmin: true);
        Assert.Null(result.ExpectQueryResult().Errors);
    }

    [Theory]
    [InlineData("""
        {
            foos(
                where: {
                    or: [
                        { field2: { eq: "baz" } },
                        { bar2: { field5: { eq: "qux" } } }
                    ]
                }
            ) { field1 }
        }
        """)]
    [InlineData("""
        {
            foos(
                where: {
                    bar2: {
                        and: [
                            { field5: { eq: "baz" } },
                            { field6: { in: ["qux", "thud"] } }
                        ]
                    }
                }
            ) { field1 }
        }
        """)]
    public async Task WhereArgumentWithoutAdminOnlyFieldsWhenNotAnAdmin_IsAuthorized(string query)
    {
        var result = await Execute(query, isAdmin: false);
        Assert.Null(result.ExpectQueryResult().Errors);
    }

    [Theory]
    [InlineData("""{ foos(where: { field1: { eq: "baz" } }) { field1 } }""")]
    [InlineData("""{ foos(where: { bar1: { field5: { eq: "baz" } } }) { field1 } }""")]
    [InlineData("""
        {
            foos(
                where: {
                    or: [
                        { field2: { eq: "baz" } },
                        { bar2: { field4: { eq: "qux" } } }
                    ]
                }
            ) { field1 }
        }
        """)]
    [InlineData("""
        {
            foos(
                where: {
                    bar2: {
                        and: [
                            { field4: { eq: "baz" } },
                            { field6: { in: ["qux", "thud"] } }
                        ]
                    }
                }
            ) { field1 }
        }
        """)]
    public async Task WhereArgumentWithAdminOnlyFieldsWhenNotAnAdmin_IsNotAuthorized(string query)
    {
        var result = await Execute(query, isAdmin: false);
        var errors = result.ExpectQueryResult().Errors;
        Assert.NotNull(errors);
        Assert.Equal(ErrorCodes.Authentication.NotAuthorized, Assert.Single(errors).Code);
    }

    [Fact]
    public async Task WhereArgumentWithAdminOnlyFieldsWhenAnAdmin_IsAuthorized()
    {
        var result = await Execute(
            """
            {
                foos(
                    where: {
                        or: [
                            { field1: { eq: "baz" } },
                            { bar1: { field4: { eq: "qux" } } }
                        ]
                    }
                ) { field1 }
            }
            """,
            isAdmin: true);
        Assert.Null(result.ExpectQueryResult().Errors);
    }

    [Fact]
    public async Task ResourceIsNotMiddlewareContext_IsNotAuthorized() =>
        Assert.False(await Authorize(new(), isAdmin: true));

    private static async Task<bool> Authorize(object resource, bool isAdmin)
    {
        var requirement = new AdminOnlyFilterAndSortFieldsRequirement();
        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(isAdmin ? [new Claim(ClaimTypes.Role, RoleNames.Admin)] : []));
        var context = new Microsoft.AspNetCore.Authorization.AuthorizationHandlerContext(
            [requirement], principal, resource);

        await requirement.HandleAsync(context);

        return context.HasSucceeded;
    }

    private static async Task<IExecutionResult> Execute(string query, bool isAdmin)
    {
        var serviceProvider = new ServiceCollection()
            .AddGraphQLServer()
            .AddAuthorizationHandler(_ => new AuthorizationHandler(isAdmin))
            .AddDirectiveType<AdminOnlyDirectiveType>()
            .AddQueryType<Query>()
            .AddFiltering(convention => convention
                .AddDefaults()
                .BindRuntimeType<Bar, BarFilterType>()
                .BindRuntimeType<Foo, FooFilterType>())
            .AddSorting(convention => convention
                .AddDefaults()
                .BindRuntimeType<Bar, BarSortType>()
                .BindRuntimeType<Foo, FooSortType>())
            .Services
            .BuildServiceProvider();

        var executor = await serviceProvider.GetRequestExecutorAsync();

        return await executor.ExecuteAsync(query);
    }

    public sealed record Bar(string Field4, string Field5, string Field6);

    public sealed class BarFilterType : FilterInputType<Bar>
    {
        protected override void Configure(IFilterInputTypeDescriptor<Bar> descriptor) =>
            descriptor.Field(x => x.Field4).Directive(AdminOnlyDirectiveType.DirectiveName);
    }

    public sealed class BarSortType : SortInputType<Bar>
    {
        protected override void Configure(ISortInputTypeDescriptor<Bar> descriptor) =>
            descriptor.Field(x => x.Field6).Directive(AdminOnlyDirectiveType.DirectiveName);
    }

    public sealed record Foo(string Field1, string Field2, string Field3, Bar Bar1, Bar Bar2);

    public sealed class FooFilterType : FilterInputType<Foo>
    {
        protected override void Configure(IFilterInputTypeDescriptor<Foo> descriptor)
        {
            descriptor.Field(x => x.Field1).Directive(AdminOnlyDirectiveType.DirectiveName);
            descriptor.Field(x => x.Bar1).Directive(AdminOnlyDirectiveType.DirectiveName);
        }
    }

    public sealed class FooSortType : SortInputType<Foo>
    {
        protected override void Configure(ISortInputTypeDescriptor<Foo> descriptor)
        {
            descriptor.Field(x => x.Field3).Directive(AdminOnlyDirectiveType.DirectiveName);
            descriptor.Field(x => x.Bar2).Directive(AdminOnlyDirectiveType.DirectiveName);
        }
    }

    public sealed class Query
    {
        [Authorize]
        [UseFiltering]
        [UseSorting]
        public IEnumerable<Foo> Foos() => [];
    }

    private sealed class AuthorizationHandler(bool isAdmin) : IAuthorizationHandler
    {
        public async ValueTask<AuthorizeResult> AuthorizeAsync(
            IMiddlewareContext context,
            AuthorizeDirective directive,
            CancellationToken cancellationToken = default) =>
            await Authorize(context, isAdmin) ?
                AuthorizeResult.Allowed :
                AuthorizeResult.NotAllowed;

        public ValueTask<AuthorizeResult> AuthorizeAsync(
            AuthorizationContext context,
            IReadOnlyList<AuthorizeDirective> directives,
            CancellationToken cancellationToken = default) =>
            throw new NotImplementedException();
    }
}
