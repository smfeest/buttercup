using Buttercup.Application;
using Buttercup.EntityModel;

namespace Buttercup.Web.Models.Recipes;

public sealed class ShowRecipeViewModel(
    Recipe recipe, Comment[] comments, CommentAttributes newCommentAttributes)
{
    public Recipe Recipe { get; } = recipe;

    public Comment[] Comments { get; } = comments;

    public IEnumerable<CommentViewModel> CommentViewModels { get; } =
        comments.Select(comment => new CommentViewModel(comment));

    public CommentAttributes NewCommentAttributes { get; } = newCommentAttributes;
}
