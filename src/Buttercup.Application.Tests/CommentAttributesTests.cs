using Buttercup.TestUtils;
using Xunit;

namespace Buttercup.Application;

public sealed class CommentAttributesTests
{
    #region Constructor(Comment)

    [Fact]
    public void Constructor_CopiesValuesFromComment()
    {
        var modelFactory = new ModelFactory();
        var comment = modelFactory.BuildComment(modelFactory.BuildRecipe());

        var attributes = new CommentAttributes(comment);

        Assert.Equal(comment.Body, attributes.Body);
    }

    #endregion
}
