using Buttercup.TestUtils;
using Xunit;

namespace Buttercup.Web.Models;

public sealed class CommentViewModelTests
{
    private readonly ModelFactory modelFactory = new();

    [Fact]
    public void DelegatesToComment()
    {
        var comment = this.modelFactory.BuildComment(
            this.modelFactory.BuildRecipe(), setOptionalAttributes: true);
        var viewModel = new CommentViewModel(comment);

        Assert.Equal(comment.Id, viewModel.Id);
        Assert.Equal(comment.Author!.Name, viewModel.AuthorName);
        Assert.Equal(comment.Created, viewModel.Created);
        Assert.Equal(comment.Body, viewModel.Body);
    }

    [Fact]
    public void AuthorName_NullWhenAuthorIsNull()
    {
        var comment = this.modelFactory.BuildComment(
            this.modelFactory.BuildRecipe(), setOptionalAttributes: false);
        var viewModel = new CommentViewModel(comment);
        Assert.Null(viewModel.AuthorName);
    }

    [Fact]
    public void IncludedEditedMarker_ReturnsFalseWhenCreatedAndModifiedMatch()
    {
        var comment = this.modelFactory.BuildComment(this.modelFactory.BuildRecipe());
        comment.Modified = comment.Created;

        var viewModel = new CommentViewModel(comment);

        Assert.False(viewModel.IncludeEditedMarker);
    }

    [Fact]
    public void IncludedEditedMarker_ReturnsTrueWhenCreatedAndModifiedDiffer()
    {
        var comment = this.modelFactory.BuildComment(this.modelFactory.BuildRecipe());
        comment.Modified = this.modelFactory.NextDateTime();

        var viewModel = new CommentViewModel(comment);

        Assert.True(viewModel.IncludeEditedMarker);
    }
}
