using Buttercup.Application;
using Buttercup.EntityModel;
using Buttercup.TestUtils;
using Buttercup.Web.Controllers.Queries;
using Buttercup.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Buttercup.Web.Controllers;

public sealed class CommentsControllerTests : IDisposable
{
    private readonly ModelFactory modelFactory = new();

    private readonly Mock<IAuthorizationService> authorizationServiceMock = new();
    private readonly Mock<ICommentManager> commentManagerMock = new();
    private readonly FakeDbContextFactory dbContextFactory = new();
    private readonly DefaultHttpContext httpContext = new();
    private readonly Mock<ICommentsControllerQueries> queriesMock = new();

    private readonly CommentsController commentsController;

    public CommentsControllerTests() =>
        this.commentsController = new(
            this.authorizationServiceMock.Object,
            this.commentManagerMock.Object,
            this.dbContextFactory,
            this.queriesMock.Object)
        {
            ControllerContext = new() { HttpContext = this.httpContext },
        };

    public void Dispose() => this.commentsController.Dispose();

    #region Delete (GET)

    [Fact]
    public async Task Delete_Get_ReturnsViewResultWithComment()
    {
        var comment = this.modelFactory.BuildComment();
        this.SetupFindCommentWithAuthor(comment.Id, comment);

        var result = await this.commentsController.Delete(comment.Id);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(comment, viewResult.Model);
    }

    [Fact]
    public async Task Delete_Get_CommentNotFoundOrAlreadySoftDeleted_ReturnsNotFoundResult()
    {
        var commentId = this.modelFactory.NextInt();
        this.SetupFindCommentWithAuthor(commentId, null);

        var result = await this.commentsController.Delete(commentId);
        Assert.IsType<NotFoundResult>(result);
    }

    #endregion

    #region Delete (POST)

    [Fact]
    public async Task Delete_Post_DeletesCommentAndRedirectsToRecipeShowPage()
    {
        var currentUserId = this.SetupCurrentUserId();
        var comment = this.modelFactory.BuildComment();
        this.SetupFindComment(comment.Id, comment);
        this.SetupAuthorizeCommentAuthorOrAdmin(comment, true);
        this.commentManagerMock
            .Setup(x => x.DeleteComment(comment.Id, currentUserId))
            .ReturnsAsync(true)
            .Verifiable();

        var result = await this.commentsController.DeletePost(comment.Id);

        this.commentManagerMock.Verify();

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Recipes", redirectResult.ControllerName);
        Assert.Equal(nameof(RecipesController.Show), redirectResult.ActionName);
        Assert.Equal(comment.RecipeId, redirectResult.RouteValues?["id"]);
    }

    [Fact]
    public async Task Delete_Post_RecipeNotFoundOrAlreadySoftDeleted_ReturnsNotFoundResult()
    {
        var commentId = this.modelFactory.NextInt();
        this.SetupFindComment(commentId, null);

        var result = await this.commentsController.DeletePost(commentId);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_Post_FailedAuthorization_ReturnsUnauthorizedResult()
    {
        this.SetupCurrentUserId();
        var comment = this.modelFactory.BuildComment();
        this.SetupFindComment(comment.Id, comment);
        this.SetupAuthorizeCommentAuthorOrAdmin(comment, false);

        var result = await this.commentsController.DeletePost(comment.Id);

        Assert.IsType<UnauthorizedResult>(result);
    }

    #endregion

    private void SetupAuthorizeCommentAuthorOrAdmin(Comment comment, bool succeeded) =>
        this.authorizationServiceMock
            .Setup(x => x.AuthorizeAsync(
                this.httpContext.User, comment, AuthorizationPolicyNames.CommentAuthorOrAdmin))
            .ReturnsAsync(succeeded ? AuthorizationResult.Success : AuthorizationResult.Failed);

    private long SetupCurrentUserId()
    {
        var userId = this.modelFactory.NextInt();
        this.httpContext.User = PrincipalFactory.CreateWithUserId(userId);
        return userId;
    }

    private void SetupFindComment(long id, Comment? comment) =>
        this.queriesMock
            .Setup(x => x.FindComment(this.dbContextFactory.FakeDbContext, id))
            .ReturnsAsync(comment);

    private void SetupFindCommentWithAuthor(long id, Comment? comment) =>
        this.queriesMock
            .Setup(x => x.FindCommentWithAuthor(this.dbContextFactory.FakeDbContext, id))
            .ReturnsAsync(comment);
}
