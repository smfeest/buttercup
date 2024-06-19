using System.Security.Claims;
using Buttercup.Application;
using Buttercup.EntityModel;
using Buttercup.Security;

namespace Buttercup.Web.Models.Recipes;

public sealed class ShowRecipeViewModel(
    Recipe recipe, Comment[] comments, CommentAttributes newCommentAttributes, ClaimsPrincipal user)
{
    public Recipe Recipe { get; } = recipe;

    public Comment[] Comments { get; } = comments;

    public IEnumerable<CommentViewModel> CommentViewModels { get; } =
        InitializeCommentViewModels(comments, user);

    public CommentAttributes NewCommentAttributes { get; } = newCommentAttributes;

    public ClaimsPrincipal User { get; } = user;

    private static IEnumerable<CommentViewModel> InitializeCommentViewModels(
        IEnumerable<Comment> comments, ClaimsPrincipal user)
    {
        var isAdmin = user.HasClaim(ClaimTypes.Role, RoleNames.Admin);
        var userId = user.TryGetUserId();

        return comments.Select(comment =>
            new CommentViewModel(
                comment,
                IncludeDeleteLink: isAdmin || comment.AuthorId == userId,
                IncludeFragmentLink: true));
    }
}
