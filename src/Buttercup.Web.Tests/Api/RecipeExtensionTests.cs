using Buttercup.TestUtils;
using Moq;
using Xunit;

namespace Buttercup.Web.Api;

public class RecipeExtensionTests
{
    #region CreatedByUser

    [Fact]
    public async Task CreatedByUserReturnsNullWhenCreatedByUserIdIsNull()
    {
        var recipe = ModelFactory.CreateRecipe() with { CreatedByUserId = null };
        var userLoader = Mock.Of<IUserLoader>(MockBehavior.Strict);

        Assert.Null(await new RecipeExtension().CreatedByUser(recipe, userLoader));
    }

    [Fact]
    public async Task CreatedByUserReturnsUserWhenCreatedByUserIdIsNotNull()
    {
        var user = ModelFactory.CreateUser();
        var recipe = ModelFactory.CreateRecipe() with { CreatedByUserId = user.Id };

        var userLoader = Mock.Of<IUserLoader>(
            x => x.LoadAsync(user.Id, default) == Task.FromResult(user));

        Assert.Equal(
            user, await new RecipeExtension().CreatedByUser(recipe, userLoader));
    }

    #endregion

    #region ModifiedByUser

    [Fact]
    public async Task ModifiedByUserReturnsNullWhenModifiedByUserIdIsNull()
    {
        var recipe = ModelFactory.CreateRecipe() with { ModifiedByUserId = null };
        var userLoader = Mock.Of<IUserLoader>(MockBehavior.Strict);

        Assert.Null(await new RecipeExtension().ModifiedByUser(recipe, userLoader));
    }

    [Fact]
    public async Task ModifiedByUserReturnsUserWhenModifiedByUserIdIsNotNull()
    {
        var user = ModelFactory.CreateUser();
        var recipe = ModelFactory.CreateRecipe() with { ModifiedByUserId = user.Id };

        var userLoader = Mock.Of<IUserLoader>(
            x => x.LoadAsync(user.Id, default) == Task.FromResult(user));

        Assert.Equal(
            user, await new RecipeExtension().ModifiedByUser(recipe, userLoader));
    }

    #endregion
}
