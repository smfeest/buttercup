using Buttercup.Application;
using Buttercup.EntityModel;

namespace Buttercup.Web.Models.Comments;

public sealed record EditCommentViewModel(
    Comment Comment, CommentAttributes Attributes, int BaseUpdateCount)
{
    public static EditCommentViewModel ForComment(Comment comment) =>
        new(comment, new(comment), comment.UpdateCount);
}
