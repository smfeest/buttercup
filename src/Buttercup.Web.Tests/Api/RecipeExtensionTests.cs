using Buttercup.DataAccess;
using Buttercup.EntityModel;
using Buttercup.TestUtils;
using Moq;
using Xunit;

namespace Buttercup.Web.Api;

public class RecipeExtensionTests
{
    private readonly ModelFactory modelFactory = new();

    #region CreatedByUser

    [Fact]
    public async Task CreatedByUserReturnsNullWhenCreatedByUserIdIsNull()
    {
        var recipe = this.modelFactory.BuildRecipe() with { CreatedByUserId = null };
        var userLoader = Mock.Of<IUsersByIdDataLoader>(MockBehavior.Strict);

        Assert.Null(await new RecipeExtension().CreatedByUser(recipe, userLoader));
    }

    [Fact]
    public async Task CreatedByUserReturnsUserWhenCreatedByUserIdIsNotNull()
    {
        var user = this.modelFactory.BuildUser();
        var recipe = this.modelFactory.BuildRecipe() with { CreatedByUserId = user.Id };

        var userLoader = Mock.Of<IUsersByIdDataLoader>(
            x => x.LoadAsync(user.Id, default) == Task.FromResult(user));

        Assert.Equal(
            user, await new RecipeExtension().CreatedByUser(recipe, userLoader));
    }

    #endregion

    #region ModifiedByUser

    [Fact]
    public async Task ModifiedByUserReturnsNullWhenModifiedByUserIdIsNull()
    {
        var recipe = this.modelFactory.BuildRecipe() with { ModifiedByUserId = null };
        var userLoader = Mock.Of<IUsersByIdDataLoader>(MockBehavior.Strict);

        Assert.Null(await new RecipeExtension().ModifiedByUser(recipe, userLoader));
    }

    [Fact]
    public async Task ModifiedByUserReturnsUserWhenModifiedByUserIdIsNotNull()
    {
        var user = this.modelFactory.BuildUser();
        var recipe = this.modelFactory.BuildRecipe() with { ModifiedByUserId = user.Id };

        var userLoader = Mock.Of<IUsersByIdDataLoader>(
            x => x.LoadAsync(user.Id, default) == Task.FromResult(user));

        Assert.Equal(
            user, await new RecipeExtension().ModifiedByUser(recipe, userLoader));
    }

    #endregion

    #region GetRecipesByIdAsync

    [Fact]
    public async void GetRecipesByIdAsyncFetchesRecipesById()
    {
        using var dbContextFactory = new FakeDbContextFactory();

        IList<Recipe> recipes = new[] { this.modelFactory.BuildRecipe(), this.modelFactory.BuildRecipe() };
        var recipeIds = recipes.Select(recipe => recipe.Id).ToArray();

        var recipeDataProvider = Mock.Of<IRecipeDataProvider>(
            x => x.GetRecipes(dbContextFactory.FakeDbContext, recipeIds) == Task.FromResult(recipes));

        var result = await RecipeExtension.GetRecipesByIdAsync(
            recipeIds, dbContextFactory, recipeDataProvider);

        Assert.Equal(result[recipeIds[0]], recipes[0]);
        Assert.Equal(result[recipeIds[1]], recipes[1]);
    }

    #endregion
}
