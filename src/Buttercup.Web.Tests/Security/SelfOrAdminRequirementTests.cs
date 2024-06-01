using System.Security.Claims;
using Buttercup.EntityModel;
using Buttercup.Security;
using Buttercup.TestUtils;
using Buttercup.Web.Api;
using HotChocolate.Resolvers;
using Microsoft.AspNetCore.Authorization;
using Moq;
using Xunit;

namespace Buttercup.Web.Security;

public sealed class SelfOrAdminRequirementTests
{
    private readonly ModelFactory modelFactory = new();

    [Fact]
    public async Task CurrentUserInAdminRole_IndicatesSuccess()
    {
        var requirement = new SelfOrAdminRequirement();
        var subjectUser = this.modelFactory.BuildUser();
        var resource = CreateMiddlewareContextWithUser(subjectUser);
        var currentUser = PrincipalFactory.CreateWithUserId(
            this.modelFactory.NextInt(), new Claim(ClaimTypes.Role, RoleNames.Admin));
        var context = new AuthorizationHandlerContext([requirement], currentUser, resource);

        await requirement.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task ResourceIsMiddlewareContextForUserObjectAndIdMatchesCurrentUser_IndicatesSuccess()
    {
        var requirement = new SelfOrAdminRequirement();
        var subjectUser = this.modelFactory.BuildUser();
        var resource = CreateMiddlewareContextWithUser(subjectUser);
        var currentUser = PrincipalFactory.CreateWithUserId(subjectUser.Id);
        var context = new AuthorizationHandlerContext([requirement], currentUser, resource);

        await requirement.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async Task ResourceIsMiddlewareContextForUserObjectAndIdDoesNotMatchCurrentUser_DoesNotIndicateSuccess()
    {
        var requirement = new SelfOrAdminRequirement();
        var subjectUser = this.modelFactory.BuildUser();
        var resource = CreateMiddlewareContextWithUser(subjectUser);
        var currentUser = PrincipalFactory.CreateWithUserId(this.modelFactory.NextInt());
        var context = new AuthorizationHandlerContext([requirement], currentUser, resource);

        await requirement.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task ResourceIsMiddlewareContextForDifferentObjectType_DoesNotIndicateSuccess()
    {
        var requirement = new SelfOrAdminRequirement();
        var resource = Mock.Of<IMiddlewareContext>(x => x.ObjectType == new RecipeType());
        var currentUser = PrincipalFactory.CreateWithUserId(this.modelFactory.NextInt());
        var context = new AuthorizationHandlerContext([requirement], currentUser, resource);

        await requirement.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task ResourceIsNotMiddlewareContext_DoesNotIndicateSuccess()
    {
        var requirement = new SelfOrAdminRequirement();
        var currentUser = PrincipalFactory.CreateWithUserId(this.modelFactory.NextInt());
        var context = new AuthorizationHandlerContext([requirement], currentUser, new());

        await requirement.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    private static IMiddlewareContext CreateMiddlewareContextWithUser(User user)
    {
        var mock = new Mock<IMiddlewareContext>();
        mock.SetupGet(x => x.ObjectType).Returns(new UserType());
        mock.Setup(x => x.Parent<User>()).Returns(user);
        return mock.Object;
    }
}
