using Buttercup.EntityModel;

namespace Buttercup.Web.Models;

public sealed record CommentViewModel(
    Comment Comment,
    bool IncludeDeleteLink = false,
    bool IncludeFragmentLink = false)
{
    public long Id => this.Comment.Id;
    public string? AuthorName => this.Comment.Author?.Name;
    public DateTime Created => this.Comment.Created;
    public string Body => this.Comment.Body;
}
