using Buttercup.TestUtils;
using Xunit;

namespace Buttercup.Web.Controllers.Queries;

[Collection(nameof(DatabaseCollection))]
public sealed class RecipesControllerQueriesTests(
    DatabaseFixture<DatabaseCollection> databaseFixture)
    : DatabaseTests<DatabaseCollection>(databaseFixture)
{
    private readonly ModelFactory modelFactory = new();
    private readonly RecipesControllerQueries queries = new(databaseFixture);

    #region FindRecipe

    [Fact]
    public async Task FindRecipe_ReturnsRecipeWithCreatedAndModifiedByUser()
    {
        var expected = this.modelFactory.BuildRecipe(setOptionalAttributes: true);

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            dbContext.Recipes.Add(expected);
            await dbContext.SaveChangesAsync();
        }

        var actual = await this.queries.FindRecipe(expected.Id, true);

        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public async Task FindRecipe_ReturnsRecipeWithoutCreatedAndModifiedByUser()
    {
        var recipe = this.modelFactory.BuildRecipe(setOptionalAttributes: true);

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            dbContext.Recipes.Add(recipe);
            await dbContext.SaveChangesAsync();
        }

        var actual = await this.queries.FindRecipe(recipe.Id, false);
        var expected = recipe with
        {
            CreatedByUser = null,
            ModifiedByUser = null,
        };

        Assert.Equivalent(expected, actual);
    }

    [Fact]
    public async Task FindRecipe_ReturnsNullIfRecordNotFound()
    {
        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            dbContext.Recipes.Add(this.modelFactory.BuildRecipe());
            await dbContext.SaveChangesAsync();
        }

        var id = this.modelFactory.NextInt();
        Assert.Null(await this.queries.FindRecipe(id));
    }

    #endregion

    #region GetRecipes

    [Fact]
    public async Task GetRecipes_ReturnsNonDeletedRecipesInTitleOrder()
    {
        var recipeB = this.modelFactory.BuildRecipe() with { Title = "recipe-title-b" };
        var recipeC = this.modelFactory.BuildRecipe() with { Title = "recipe-title-c" };
        var recipeA = this.modelFactory.BuildRecipe() with { Title = "recipe-title-a" };
        var deletedRecipe = this.modelFactory.BuildRecipe(softDeleted: true);

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            dbContext.Recipes.AddRange(recipeB, recipeC, recipeA, deletedRecipe);
            await dbContext.SaveChangesAsync();
        }

        Assert.Collection(
            await this.queries.GetRecipes(),
            r => Assert.Equivalent(recipeA, r),
            r => Assert.Equivalent(recipeB, r),
            r => Assert.Equivalent(recipeC, r));
    }

    #endregion
}
