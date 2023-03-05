using System.Security.Claims;
using Buttercup.DataAccess;
using Buttercup.Models;
using Buttercup.TestUtils;
using Moq;
using MySqlConnector;
using Xunit;

namespace Buttercup.Web.Api;

public sealed class QueryTests : IDisposable
{
    private readonly Query query;
    private readonly MySqlConnection mySqlConnection = new();

    public QueryTests()
    {
        var mySqlConnectionSource = Mock.Of<IMySqlConnectionSource>(
            x => x.OpenConnection() == Task.FromResult(this.mySqlConnection));

        this.query = new(mySqlConnectionSource);
    }

    public void Dispose() => this.mySqlConnection.Dispose();

    #region CurrentUser

    [Fact]
    public async Task CurrentUserReturnsCurrentUserWhenAuthenticated()
    {
        var user = ModelFactory.CreateUser();

        var userDataProvider = Mock.Of<IUserDataProvider>(
            x => x.GetUser(this.mySqlConnection, 1234) == Task.FromResult(user));

        var principal = new ClaimsPrincipal(
            new ClaimsIdentity(new Claim[] { new(ClaimTypes.NameIdentifier, "1234") }));

        Assert.Equal(user, await this.query.CurrentUser(userDataProvider, principal));
    }

    [Fact]
    public async Task CurrentUserReturnsNullWhenNotAuthenticated() =>
        Assert.Null(
            await this.query.CurrentUser(Mock.Of<IUserDataProvider>(), new ClaimsPrincipal()));

    #endregion

    #region Recipe

    [Fact]
    public async Task RecipeReturnsRecipe()
    {
        var recipe = ModelFactory.CreateRecipe();

        var recipeLoader = Mock.Of<IRecipeLoader>(
            x => x.LoadAsync(recipe.Id, default) == Task.FromResult(recipe));

        Assert.Equal(recipe, await this.query.Recipe(recipeLoader, recipe.Id));
    }

    #endregion

    #region Recipes

    [Fact]
    public async Task RecipesReturnsAllRecipes()
    {
        IList<Recipe> expected = new[] { ModelFactory.CreateRecipe() };

        var recipeDataProvider = Mock.Of<IRecipeDataProvider>(
            x => x.GetAllRecipes(this.mySqlConnection) == Task.FromResult(expected));

        var actual = await this.query.Recipes(recipeDataProvider);

        Assert.Equal(expected, actual);
    }

    #endregion
}
