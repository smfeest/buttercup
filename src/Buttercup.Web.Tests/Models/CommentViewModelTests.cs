using Buttercup.TestUtils;
using Xunit;

namespace Buttercup.Web.Models;

public sealed class CommentViewModelTests
{
    [Fact]
    public void DelegatesToComment()
    {
        var comment = new ModelFactory().BuildComment(setOptionalAttributes: true);
        var viewModel = new CommentViewModel(comment);

        Assert.Equal(comment.Id, viewModel.Id);
        Assert.Equal(comment.Author!.Name, viewModel.AuthorName);
        Assert.Equal(comment.Created, viewModel.Created);
        Assert.Equal(comment.Body, viewModel.Body);
    }

    [Fact]
    public void AuthorName_NullWhenAuthorIsNull()
    {
        var comment = new ModelFactory().BuildComment(setOptionalAttributes: false);
        var viewModel = new CommentViewModel(comment);
        Assert.Null(viewModel.AuthorName);
    }
}
