using Buttercup.Application;
using Buttercup.EntityModel;
using Buttercup.Security;
using Buttercup.Web.Controllers.Queries;
using Buttercup.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Buttercup.Web.Controllers;

[Authorize]
[Route("comments")]
public sealed class CommentsController(
    IAuthorizationService authorizationService,
    ICommentManager commentManager,
    IDbContextFactory<AppDbContext> dbContextFactory,
    ICommentsControllerQueries queries)
    : Controller
{
    private readonly IAuthorizationService authorizationService = authorizationService;
    private readonly ICommentManager commentManager = commentManager;
    private readonly IDbContextFactory<AppDbContext> dbContextFactory = dbContextFactory;
    private readonly ICommentsControllerQueries queries = queries;

    [HttpGet("{id}/delete")]
    public async Task<IActionResult> Delete(long id)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();
        var comment = await this.queries.FindCommentWithAuthor(dbContext, id);
        return comment is null ? this.NotFound() : this.View(comment);
    }

    [HttpPost("{id}/delete")]
    public async Task<IActionResult> DeletePost(long id)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        var comment = await this.queries.FindComment(dbContext, id);

        if (comment is null)
        {
            return this.NotFound();
        }

        var authorizationResult = await this.authorizationService.AuthorizeAsync(
            this.User, comment, AuthorizationPolicyNames.CommentAuthorOrAdmin);

        if (!authorizationResult.Succeeded)
        {
            return this.Unauthorized();
        }

        await this.commentManager.DeleteComment(id, this.User.GetUserId());

        return this.RedirectToAction(
            nameof(RecipesController.Show), "Recipes", new { id = comment.RecipeId });
    }
}
