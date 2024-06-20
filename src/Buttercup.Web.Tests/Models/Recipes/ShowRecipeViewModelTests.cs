using Buttercup.Application;
using Buttercup.TestUtils;
using Xunit;

namespace Buttercup.Web.Models.Recipes;

public sealed class ShowRecipeViewModelTests
{
    private readonly ModelFactory modelFactory = new();

    [Fact]
    public void ExposesRecipe()
    {
        var recipe = this.modelFactory.BuildRecipe();
        var viewModel = new ShowRecipeViewModel(recipe, [], new());

        Assert.Equal(recipe, viewModel.Recipe);
    }

    [Fact]
    public void ExposesComments()
    {
        var comments = new[] { this.modelFactory.BuildComment(), this.modelFactory.BuildComment() };
        var viewModel = new ShowRecipeViewModel(this.modelFactory.BuildRecipe(), comments, new());

        Assert.Equal(comments, viewModel.Comments);
    }

    [Fact]
    public void ExposesNewCommentAttributes()
    {
        var commentAttributes = new CommentAttributes
        {
            Body = this.modelFactory.NextString("comment-body")
        };
        var viewModel = new ShowRecipeViewModel(
            this.modelFactory.BuildRecipe(), [], commentAttributes);

        Assert.Equal(commentAttributes, viewModel.NewCommentAttributes);
    }

    [Fact]
    public void InitializesCommentViewModels()
    {
        var comment = this.modelFactory.BuildComment();
        var viewModel = new ShowRecipeViewModel(this.modelFactory.BuildRecipe(), [comment], new());

        Assert.Equal([new CommentViewModel(comment)], viewModel.CommentViewModels);
    }
}
