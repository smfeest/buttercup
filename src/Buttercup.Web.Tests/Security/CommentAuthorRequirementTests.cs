using Buttercup.TestUtils;
using Microsoft.AspNetCore.Authorization;
using Xunit;

namespace Buttercup.Web.Security;

public sealed class CommentAuthorRequirementTests
{
    private readonly ModelFactory modelFactory = new();

    [Fact]
    public async Task ResourceIsNotComment_DoesNotIndicateSuccess()
    {
        var requirement = new CommentAuthorRequirement();
        var currentUser = PrincipalFactory.CreateWithUserId(this.modelFactory.NextInt());
        var context = new AuthorizationHandlerContext([requirement], currentUser, new());

        await requirement.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task ResourceIsCommentAndAuthorIdIsNull_DoesNotIndicateSuccess()
    {
        var requirement = new CommentAuthorRequirement();
        var currentUser = PrincipalFactory.CreateWithUserId(this.modelFactory.NextInt());
        var resource = this.modelFactory.BuildComment(this.modelFactory.BuildRecipe()) with
        {
            AuthorId = null,
        };
        var context = new AuthorizationHandlerContext([requirement], currentUser, resource);

        await requirement.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task ResourceIsCommentAndAuthorIdDoesNotMatchCurrentUser_DoesNotIndicateSuccess()
    {
        var requirement = new CommentAuthorRequirement();
        var currentUser = PrincipalFactory.CreateWithUserId(this.modelFactory.NextInt());
        var resource = this.modelFactory.BuildComment(this.modelFactory.BuildRecipe()) with
        {
            AuthorId = this.modelFactory.NextInt(),
        };
        var context = new AuthorizationHandlerContext([requirement], currentUser, resource);

        await requirement.HandleAsync(context);

        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async Task ResourceIsCommentAndAuthorIdMatchesCurrentUser_IndicatesSuccess()
    {
        var requirement = new CommentAuthorRequirement();
        var currentUserId = this.modelFactory.NextInt();
        var currentUser = PrincipalFactory.CreateWithUserId(currentUserId);
        var resource = this.modelFactory.BuildComment(this.modelFactory.BuildRecipe()) with
        {
            AuthorId = currentUserId,
        };
        var context = new AuthorizationHandlerContext([requirement], currentUser, resource);

        await requirement.HandleAsync(context);

        Assert.True(context.HasSucceeded);
    }
}
