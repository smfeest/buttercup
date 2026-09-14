using System.Net;
using Buttercup.Application;
using Buttercup.EntityModel;
using Buttercup.TestUtils;
using Buttercup.Web.Controllers.Queries;
using Buttercup.Web.Models.Comments;
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
    private readonly DictionaryLocalizer<CommentsController> localizer = new();
    private readonly Mock<ICommentsControllerQueries> queriesMock = new();

    private readonly CommentsController commentsController;

    public CommentsControllerTests() =>
        this.commentsController = new(
            this.authorizationServiceMock.Object,
            this.commentManagerMock.Object,
            this.dbContextFactory,
            this.localizer,
            this.queriesMock.Object)
        {
            ControllerContext = new() { HttpContext = this.httpContext },
        };

    public void Dispose() => this.commentsController.Dispose();

    #region Delete (GET)

    [Fact]
    public async Task Delete_Get_ReturnsViewResultWithComment()
    {
        var comment = this.modelFactory.BuildComment(this.modelFactory.BuildRecipe());
        this.SetupFindCommentWithAuthor(comment.Id, comment);
        this.SetupAuthorize(comment, AuthorizationPolicyNames.CommentAuthorOrAdmin, true);

        var result = await this.commentsController.Delete(
            comment.Id, TestContext.Current.CancellationToken);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(comment, viewResult.Model);
    }

    [Fact]
    public async Task Delete_Get_CommentNotFoundOrAlreadySoftDeleted_ReturnsNotFoundResult()
    {
        var commentId = this.modelFactory.NextInt();
        this.SetupFindCommentWithAuthor(commentId, null);

        var result = await this.commentsController.Delete(
            commentId, TestContext.Current.CancellationToken);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_Get_FailedAuthorization_ReturnsForbidResult()
    {
        var comment = this.modelFactory.BuildComment(this.modelFactory.BuildRecipe());
        this.SetupFindCommentWithAuthor(comment.Id, comment);
        this.SetupAuthorize(comment, AuthorizationPolicyNames.CommentAuthorOrAdmin, false);

        var result = await this.commentsController.Delete(
            comment.Id, TestContext.Current.CancellationToken);

        Assert.IsType<ForbidResult>(result);
    }

    #endregion

    #region Delete (POST)

    [Fact]
    public async Task Delete_Post_DeletesCommentAndRedirectsToRecipeShowPage()
    {
        var currentUserId = this.SetupCurrentUserId();
        var comment = this.SetupFindComment();
        var ipAddress = this.SetupRemoteIpAddress();
        this.SetupAuthorize(comment, AuthorizationPolicyNames.CommentAuthorOrAdmin, true);
        this.commentManagerMock
            .Setup(x => x.DeleteComment(
                comment.Id, currentUserId, ipAddress, TestContext.Current.CancellationToken))
            .ReturnsAsync(true)
            .Verifiable();

        var result = await this.commentsController.DeletePost(
            comment.Id, TestContext.Current.CancellationToken);

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

        var result = await this.commentsController.DeletePost(
            commentId, TestContext.Current.CancellationToken);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_Post_FailedAuthorization_ReturnsForbidResult()
    {
        this.SetupCurrentUserId();
        var comment = this.SetupFindComment();
        this.SetupAuthorize(comment, AuthorizationPolicyNames.CommentAuthorOrAdmin, false);

        var result = await this.commentsController.DeletePost(
            comment.Id, TestContext.Current.CancellationToken);

        Assert.IsType<ForbidResult>(result);
    }

    #endregion

    #region Edit (GET)

    [Fact]
    public async Task Edit_Get_ReturnsViewResultWithEditModel()
    {
        this.SetupCurrentUserId();
        var comment = this.SetupFindComment();
        this.SetupAuthorize(comment, AuthorizationPolicyNames.CommentAuthor, true);

        var result = await this.commentsController.Edit(
            comment.Id, TestContext.Current.CancellationToken);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<EditCommentViewModel>(viewResult.Model);
        Assert.Equal(EditCommentViewModel.ForComment(comment), model);
    }

    [Fact]
    public async Task Edit_Get_CommentNotFoundOrAlreadySoftDeleted_ReturnsNotFoundResult()
    {
        var commentId = this.modelFactory.NextInt();
        this.SetupFindComment(commentId, null);

        var result = await this.commentsController.Edit(
            commentId, TestContext.Current.CancellationToken);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Get_FailedAuthorization_ReturnsForbidResult()
    {
        this.SetupCurrentUserId();
        var comment = this.SetupFindComment();
        this.SetupAuthorize(comment, AuthorizationPolicyNames.CommentAuthor, false);

        var result = await this.commentsController.Edit(
            comment.Id, TestContext.Current.CancellationToken);

        Assert.IsType<ForbidResult>(result);
    }

    #endregion

    #region Edit (POST)

    [Fact]
    public async Task Edit_Post_Success_UpdatesCommentAndRedirectsToRecipeShowPage()
    {
        var currentUserId = this.SetupCurrentUserId();
        var comment = this.SetupFindComment();
        var attributes = this.BuildCommentAttributes();
        var baseUpdateCount = this.modelFactory.NextInt();
        var ipAddress = this.SetupRemoteIpAddress();
        this.SetupAuthorize(comment, AuthorizationPolicyNames.CommentAuthor, true);

        var result = await this.commentsController.Edit(
            comment.Id, attributes, baseUpdateCount, TestContext.Current.CancellationToken);

        this.commentManagerMock.Verify(
            x => x.UpdateComment(
                comment.Id,
                attributes,
                baseUpdateCount,
                currentUserId,
                ipAddress,
                TestContext.Current.CancellationToken));

        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(RecipesController.Show), redirectResult.ActionName);
        Assert.Equal("Recipes", redirectResult.ControllerName);
        Assert.NotNull(redirectResult.RouteValues);
        Assert.Equal(comment.RecipeId, redirectResult.RouteValues["id"]);
        Assert.Equal($"comment{comment.Id}", redirectResult.Fragment);
    }

    [Fact]
    public async Task Edit_Post_CommentNotFoundOrAlreadySoftDeleted_ReturnsNotFoundResult()
    {
        var commentId = this.modelFactory.NextInt();
        this.SetupFindComment(commentId, null);

        var result = await this.commentsController.Edit(
            commentId, this.BuildCommentAttributes(), 0, TestContext.Current.CancellationToken);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Post_FailedAuthorization_ReturnsForbidResult()
    {
        this.SetupCurrentUserId();
        var comment = this.SetupFindComment();
        this.SetupAuthorize(comment, AuthorizationPolicyNames.CommentAuthor, false);

        var result = await this.commentsController.Edit(
            comment.Id,
            this.BuildCommentAttributes(),
            this.modelFactory.NextInt(),
            TestContext.Current.CancellationToken);

        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task Edit_Post_InvalidModel_ReturnsViewResultWithEditModel()
    {
        this.SetupCurrentUserId();
        var comment = this.SetupFindComment();
        var attributes = this.BuildCommentAttributes();
        var baseUpdateCount = this.modelFactory.NextInt();
        this.SetupAuthorize(comment, AuthorizationPolicyNames.CommentAuthor, true);

        this.commentsController.ModelState.AddModelError("test", "test");

        var result = await this.commentsController.Edit(
            comment.Id, attributes, baseUpdateCount, TestContext.Current.CancellationToken);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<EditCommentViewModel>(viewResult.Model);
        Assert.Equal(new(comment, attributes, baseUpdateCount), model);
    }

    [Fact]
    public async Task Edit_Post_ConcurrencyException_ReturnsViewResultAndAddsError()
    {
        var currentUserId = this.SetupCurrentUserId();
        var comment = this.SetupFindComment();
        var attributes = this.BuildCommentAttributes();
        var baseUpdateCount = this.modelFactory.NextInt();
        var ipAddress = this.SetupRemoteIpAddress();
        this.SetupAuthorize(comment, AuthorizationPolicyNames.CommentAuthor, true);

        this.localizer.Add("Error_StaleEdit", "translated-stale-edit-error");

        this.commentManagerMock
            .Setup(
                x => x.UpdateComment(
                    comment.Id,
                    attributes,
                    baseUpdateCount,
                    currentUserId,
                    ipAddress,
                    TestContext.Current.CancellationToken))
            .ThrowsAsync(new ConcurrencyException());

        var result = await this.commentsController.Edit(
            comment.Id, attributes, baseUpdateCount, TestContext.Current.CancellationToken);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<EditCommentViewModel>(viewResult.Model);
        Assert.Equal(new(comment, attributes, baseUpdateCount), model);

        var formState = this.commentsController.ModelState[string.Empty];
        Assert.NotNull(formState);
        var error = Assert.Single(formState.Errors);
        Assert.Equal("translated-stale-edit-error", error.ErrorMessage);
    }

    [Fact]
    public async Task Edit_Post_NotFoundException_ReturnsNotFoundResult()
    {
        var currentUserId = this.SetupCurrentUserId();
        var comment = this.SetupFindComment();
        var attributes = this.BuildCommentAttributes();
        var baseUpdateCount = this.modelFactory.NextInt();
        var ipAddress = this.SetupRemoteIpAddress();
        this.SetupAuthorize(comment, AuthorizationPolicyNames.CommentAuthor, true);

        this.commentManagerMock
            .Setup(
                x => x.UpdateComment(
                    comment.Id,
                    attributes,
                    baseUpdateCount,
                    currentUserId,
                    ipAddress,
                    TestContext.Current.CancellationToken))
            .ThrowsAsync(new NotFoundException());

        var result = await this.commentsController.Edit(
            comment.Id, attributes, baseUpdateCount, TestContext.Current.CancellationToken);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Edit_Post_SoftDeletedException_ReturnsNotFoundResult()
    {
        var currentUserId = this.SetupCurrentUserId();
        var comment = this.SetupFindComment();
        var attributes = this.BuildCommentAttributes();
        var baseUpdateCount = this.modelFactory.NextInt();
        var ipAddress = this.SetupRemoteIpAddress();
        this.SetupAuthorize(comment, AuthorizationPolicyNames.CommentAuthor, true);

        this.commentManagerMock
            .Setup(
                x => x.UpdateComment(
                    comment.Id,
                    attributes,
                    baseUpdateCount,
                    currentUserId,
                    ipAddress,
                    TestContext.Current.CancellationToken))
            .ThrowsAsync(new SoftDeletedException());

        var result = await this.commentsController.Edit(
            comment.Id, attributes, baseUpdateCount, TestContext.Current.CancellationToken);

        Assert.IsType<NotFoundResult>(result);
    }

    #endregion

    private CommentAttributes BuildCommentAttributes() =>
        new() { Body = this.modelFactory.NextString("comment-body") };

    private void SetupAuthorize(Comment comment, string policyName, bool succeeded) =>
        this.authorizationServiceMock
            .Setup(x => x.AuthorizeAsync(this.httpContext.User, comment, policyName))
            .ReturnsAsync(succeeded ? AuthorizationResult.Success : AuthorizationResult.Failed);

    private long SetupCurrentUserId()
    {
        var userId = this.modelFactory.NextInt();
        this.httpContext.User = PrincipalFactory.CreateWithUserId(userId);
        return userId;
    }

    private Comment SetupFindComment()
    {
        var comment = this.modelFactory.BuildComment(this.modelFactory.BuildRecipe());
        this.SetupFindComment(comment.Id, comment);
        return comment;
    }

    private void SetupFindComment(long id, Comment? comment) =>
        this.queriesMock
            .Setup(x => x.FindComment(
                this.dbContextFactory.FakeDbContext, id, TestContext.Current.CancellationToken))
            .ReturnsAsync(comment);

    private void SetupFindCommentWithAuthor(long id, Comment? comment) =>
        this.queriesMock
            .Setup(x => x.FindCommentWithAuthor(
                this.dbContextFactory.FakeDbContext, id, TestContext.Current.CancellationToken))
            .ReturnsAsync(comment);

    private IPAddress SetupRemoteIpAddress()
    {
        var ipAddress = this.modelFactory.NextIpAddress();
        this.httpContext.SetRemoteIpAddress(ipAddress);
        return ipAddress;
    }
}
