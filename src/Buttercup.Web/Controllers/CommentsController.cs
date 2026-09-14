using Buttercup.Application;
using Buttercup.EntityModel;
using Buttercup.Security;
using Buttercup.Web.Controllers.Queries;
using Buttercup.Web.Models.Comments;
using Buttercup.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Buttercup.Web.Controllers;

[Authorize]
[Route("comments")]
public sealed class CommentsController(
    IAuthorizationService authorizationService,
    ICommentManager commentManager,
    IDbContextFactory<AppDbContext> dbContextFactory,
    IStringLocalizer<CommentsController> localizer,
    ICommentsControllerQueries queries)
    : Controller
{
    private readonly IAuthorizationService authorizationService = authorizationService;
    private readonly ICommentManager commentManager = commentManager;
    private readonly IDbContextFactory<AppDbContext> dbContextFactory = dbContextFactory;
    private readonly IStringLocalizer<CommentsController> localizer = localizer;
    private readonly ICommentsControllerQueries queries = queries;

    [HttpGet("{id}/delete")]
    public async Task<IActionResult> Delete(long id, CancellationToken cancellationToken)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        var comment = await this.queries.FindCommentWithAuthor(dbContext, id, cancellationToken);

        if (comment is null)
        {
            return this.NotFound();
        }

        if (!await this.UserAuthorisedToDelete(comment))
        {
            return this.Forbid();
        }

        return this.View(comment);
    }

    [HttpPost("{id}/delete")]
    public async Task<IActionResult> DeletePost(long id, CancellationToken cancellationToken)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        var comment = await this.queries.FindComment(dbContext, id, cancellationToken);

        if (comment is null)
        {
            return this.NotFound();
        }

        if (!await this.UserAuthorisedToDelete(comment))
        {
            return this.Forbid();
        }

        await this.commentManager.DeleteComment(
            id,
            this.User.GetUserId(),
            this.HttpContext.Connection.RemoteIpAddress,
            cancellationToken);

        return this.RedirectToAction(
            nameof(RecipesController.Show), "Recipes", new { id = comment.RecipeId });
    }

    [HttpGet("{id}/edit")]
    public async Task<IActionResult> Edit(long id, CancellationToken cancellationToken)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        var comment = await this.queries.FindComment(dbContext, id, cancellationToken);

        if (comment is null)
        {
            return this.NotFound();
        }

        if (!await this.UserAuthorisedToEdit(comment))
        {
            return this.Forbid();
        }

        return this.View(EditCommentViewModel.ForComment(comment));
    }

    [HttpPost("{id}/edit")]
    public async Task<IActionResult> Edit(
        long id,
        CommentAttributes attributes,
        int baseUpdateCount,
        CancellationToken cancellationToken)
    {
        using var dbContext = this.dbContextFactory.CreateDbContext();

        var comment = await this.queries.FindComment(dbContext, id, cancellationToken);

        if (comment is null)
        {
            return this.NotFound();
        }

        if (!await this.UserAuthorisedToEdit(comment))
        {
            return this.Forbid();
        }

        if (!this.ModelState.IsValid)
        {
            return this.View(new EditCommentViewModel(comment, attributes, baseUpdateCount));
        }

        try
        {
            await this.commentManager.UpdateComment(
                id,
                attributes,
                baseUpdateCount,
                this.User.GetUserId(),
                this.HttpContext.Connection.RemoteIpAddress,
                cancellationToken);
        }
        catch (ConcurrencyException)
        {
            this.ModelState.AddModelError(string.Empty, this.localizer["Error_StaleEdit"]);

            return this.View(new EditCommentViewModel(comment, attributes, baseUpdateCount));
        }
        catch (Exception e) when (e is NotFoundException or SoftDeletedException)
        {
            return this.NotFound();
        }

        return this.RedirectToAction(
            nameof(RecipesController.Show), "Recipes", new { id = comment.RecipeId }, $"comment{id}");
    }

    private async Task<bool> UserAuthorisedToDelete(Comment comment)
    {
        var authorizationResult = await this.authorizationService.AuthorizeAsync(
            this.User, comment, AuthorizationPolicyNames.CommentAuthorOrAdmin);

        return authorizationResult.Succeeded;
    }

    private async Task<bool> UserAuthorisedToEdit(Comment comment)
    {
        var authorizationResult = await this.authorizationService.AuthorizeAsync(
            this.User, comment, AuthorizationPolicyNames.CommentAuthor);

        return authorizationResult.Succeeded;
    }
}
