using System.Security.Claims;
using Buttercup.EntityModel;
using Buttercup.Security;
using HotChocolate.Resolvers;
using Microsoft.AspNetCore.Authorization;
using Moq;
using Xunit;

namespace Buttercup.Web.Security;

public sealed class AdminWhenDeletedRequirementTests
{
    [Theory]
    [MemberData(nameof(GetTheoryDataForResourceNotDeleted))]
    public async Task ResourceNotDeletedAndCurrentUserNotInAdminRole_IndicatesSuccess(
        object resource)
    {
        var requirement = new AdminWhenDeletedRequirement();
        var context = new AuthorizationHandlerContext([requirement], new(), resource);

        await requirement.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Theory]
    [MemberData(nameof(GetTheoryDataForResourceDeleted))]
    public async Task ResourceDeletedAndCurrentUserNotInAdminRole_DoesNotIndicateSuccess(
        object resource)
    {
        var requirement = new AdminWhenDeletedRequirement();
        var context = new AuthorizationHandlerContext([requirement], new(), resource);

        await requirement.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Theory]
    [MemberData(nameof(GetTheoryDataForResourceDeleted))]
    public async Task ResourceDeletedAndCurrentUserInAdminRole_IndicatesSuccess(object resource)
    {
        var requirement = new AdminWhenDeletedRequirement();
        var currentUser = new ClaimsPrincipal(
            new ClaimsIdentity([new Claim(ClaimTypes.Role, RoleNames.Admin)]));
        var context = new AuthorizationHandlerContext([requirement], currentUser, resource);

        await requirement.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task ResourceNullAndCurrentUserNotInAdminRole_IndicatesSuccess()
    {
        var requirement = new AdminWhenDeletedRequirement();
        var middlewareContext = Mock.Of<IMiddlewareContext>(x => x.Result == null);
        var context = new AuthorizationHandlerContext([requirement], new(), middlewareContext);

        await requirement.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    public static TheoryData<object> GetTheoryDataForResourceDeleted() =>
        GetTheoryDataForResourceState(true);

    public static TheoryData<object> GetTheoryDataForResourceNotDeleted() =>
        GetTheoryDataForResourceState(false);

    private static TheoryData<object> GetTheoryDataForResourceState(bool deleted)
    {
        var deletable = Mock.Of<ISoftDeletable>(
            x => x.Deleted == (deleted ? DateTime.UtcNow : null));
        return new([deletable, Mock.Of<IMiddlewareContext>(x => x.Result == deletable)]);
    }
}
