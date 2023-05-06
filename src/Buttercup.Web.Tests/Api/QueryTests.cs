using System.Security.Claims;
using Buttercup.DataAccess;
using Buttercup.EntityModel;
using Buttercup.TestUtils;
using Moq;
using MySqlConnector;
using Xunit;

namespace Buttercup.Web.Api;

public sealed class QueryTests : IDisposable
{
    private readonly FakeDbContextFactory dbContextFactory = new();
    private readonly Query query;
    private readonly ModelFactory modelFactory = new();
    private readonly MySqlConnection mySqlConnection = new();

    public QueryTests()
    {
        var mySqlConnectionSource = Mock.Of<IMySqlConnectionSource>(
            x => x.OpenConnection() == Task.FromResult(this.mySqlConnection));

        this.query = new(dbContextFactory, mySqlConnectionSource);
    }

    public void Dispose()
    {
        this.dbContextFactory.Dispose();
        this.mySqlConnection.Dispose();
    }

    #region CurrentUser

    [Fact]
    public async Task CurrentUserReturnsCurrentUserWhenAuthenticated()
    {
        var user = this.modelFactory.BuildUser();

        var userDataProvider = Mock.Of<IUserDataProvider>(
            x => x.GetUser(this.dbContextFactory.FakeDbContext, 1234) == Task.FromResult(user));

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
        var recipe = this.modelFactory.BuildRecipe();

        var recipeLoader = Mock.Of<IRecipesByIdDataLoader>(
            x => x.LoadAsync(recipe.Id, default) == Task.FromResult(recipe));

        Assert.Equal(recipe, await this.query.Recipe(recipeLoader, recipe.Id));
    }

    #endregion

    #region Recipes

    [Fact]
    public async Task RecipesReturnsAllRecipes()
    {
        IList<Recipe> expected = new[] { this.modelFactory.BuildRecipe() };

        var recipeDataProvider = Mock.Of<IRecipeDataProvider>(
            x => x.GetAllRecipes(this.mySqlConnection) == Task.FromResult(expected));

        var actual = await this.query.Recipes(recipeDataProvider);

        Assert.Equal(expected, actual);
    }

    #endregion
}
