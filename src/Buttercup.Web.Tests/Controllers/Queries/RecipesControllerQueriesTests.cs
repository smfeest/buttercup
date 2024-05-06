using Buttercup.TestUtils;
using Xunit;

namespace Buttercup.Web.Controllers.Queries;

[Collection(nameof(DatabaseCollection))]
public sealed class RecipesControllerQueriesTests(
    DatabaseFixture<DatabaseCollection> databaseFixture)
    : DatabaseTests<DatabaseCollection>(databaseFixture)
{
    private readonly ModelFactory modelFactory = new();
    private readonly RecipesControllerQueries queries = new();

    #region FindRecipe

    [Fact]
    public async Task FindRecipe_ReturnsRecipe()
    {
        var recipe = this.modelFactory.BuildRecipe(setOptionalAttributes: true);
        await this.DatabaseFixture.InsertEntities(recipe);

        using var dbContext = this.DatabaseFixture.CreateDbContext();
        var actual = await this.queries.FindRecipe(dbContext, recipe.Id);
        var expected = recipe with
        {
            CreatedByUser = null,
            ModifiedByUser = null,
        };

        Assert.Equivalent(expected, actual);
    }

    #endregion

    #region FindRecipeForShowView

    [Fact]
    public async Task FindRecipeForShowView_ReturnsRecipeWithCreatedAndModifiedByUser()
    {
        var expected = this.modelFactory.BuildRecipe(setOptionalAttributes: true);
        await this.DatabaseFixture.InsertEntities(expected);

        using var dbContext = this.DatabaseFixture.CreateDbContext();
        var actual = await this.queries.FindRecipeForShowView(dbContext, expected.Id);

        Assert.Equivalent(expected, actual);
    }

    #endregion

    #region GetRecipesForIndex

    [Fact]
    public async Task GetRecipesForIndex_ReturnsNonDeletedRecipesInTitleOrder()
    {
        var recipeB = this.modelFactory.BuildRecipe() with { Title = "recipe-title-b" };
        var recipeC = this.modelFactory.BuildRecipe() with { Title = "recipe-title-c" };
        var recipeA = this.modelFactory.BuildRecipe() with { Title = "recipe-title-a" };
        var deletedRecipe = this.modelFactory.BuildRecipe(softDeleted: true);
        await this.DatabaseFixture.InsertEntities(recipeB, recipeC, recipeA, deletedRecipe);

        using var dbContext = this.DatabaseFixture.CreateDbContext();

        Assert.Collection(
            await this.queries.GetRecipesForIndex(dbContext),
            r => Assert.Equivalent(recipeA, r),
            r => Assert.Equivalent(recipeB, r),
            r => Assert.Equivalent(recipeC, r));
    }

    #endregion
}
