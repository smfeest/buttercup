using System.Security.Claims;
using Buttercup.Application;
using Buttercup.EntityModel;
using Buttercup.Security;
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
        var viewModel = new ShowRecipeViewModel(recipe, [], new(), new());

        Assert.Equal(recipe, viewModel.Recipe);
    }

    [Fact]
    public void ExposesComments()
    {
        var recipe = this.modelFactory.BuildRecipe();
        var comments = new[]
        {
            this.modelFactory.BuildComment(recipe),
            this.modelFactory.BuildComment(recipe),
        };
        var viewModel = new ShowRecipeViewModel(recipe, comments, new(), new());

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
            this.modelFactory.BuildRecipe(), [], commentAttributes, new());

        Assert.Equal(commentAttributes, viewModel.NewCommentAttributes);
    }

    [Fact]
    public void ExposesUser()
    {
        var user = new ClaimsPrincipal();
        var viewModel = new ShowRecipeViewModel(this.modelFactory.BuildRecipe(), [], new(), user);

        Assert.Equal(user, viewModel.User);
    }

    [Fact]
    public void UserNotAnAdmin_InitializesCommentViewModelsWithDeleteLinkOnOwnComments()
    {
        var userId = this.modelFactory.NextInt();
        var recipe = this.modelFactory.BuildRecipe();
        var comments = new Comment[]
        {
            this.modelFactory.BuildComment(recipe),
            this.modelFactory.BuildComment(recipe) with { AuthorId = userId },
            this.modelFactory.BuildComment(recipe) with { AuthorId = this.modelFactory.NextInt() },
        };
        var user = PrincipalFactory.CreateWithUserId(userId);
        var viewModel = new ShowRecipeViewModel(recipe, comments, new(), user);

        Assert.Equal(
            [
                new(comments[0], IncludeDeleteLink: false, IncludeFragmentLink: true),
                new(comments[1], IncludeDeleteLink: true, IncludeFragmentLink: true),
                new(comments[2], IncludeDeleteLink: false, IncludeFragmentLink: true),
            ],
            viewModel.CommentViewModels);
    }

    [Fact]
    public void UserIsAdmin_InitializesCommentViewModelsWithDeleteLinkOnAllComments()
    {
        var recipe = this.modelFactory.BuildRecipe();
        var comments = new Comment[]
        {
            this.modelFactory.BuildComment(recipe),
            this.modelFactory.BuildComment(recipe) with { AuthorId = this.modelFactory.NextInt() },
        };
        var user = PrincipalFactory.CreateWithUserId(
            this.modelFactory.NextInt(), new Claim(ClaimTypes.Role, RoleNames.Admin));
        var viewModel = new ShowRecipeViewModel(recipe, comments, new(), user);

        Assert.Equal(
            [
                new(comments[0], IncludeDeleteLink: true, IncludeFragmentLink: true),
                new(comments[1], IncludeDeleteLink: true, IncludeFragmentLink: true)
            ],
            viewModel.CommentViewModels);
    }
}
