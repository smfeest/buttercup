using Buttercup.TestUtils;
using Xunit;

namespace Buttercup.Web.Models.Comments;

public sealed class EditCommentViewModelTests
{
    #region ForComment

    [Fact]
    public void ForComment_CopiesValuesFromComment()
    {
        var modelFactory = new ModelFactory();
        var comment = modelFactory.BuildComment(modelFactory.BuildRecipe());

        var editModel = EditCommentViewModel.ForComment(comment);

        Assert.Equal(comment, editModel.Comment);
        Assert.Equal(new(comment), editModel.Attributes);
        Assert.Equal(comment.UpdateCount, editModel.BaseUpdateCount);
    }

    #endregion
}
