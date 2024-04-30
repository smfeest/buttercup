using Buttercup.EntityModel;
using Buttercup.TestUtils;
using Xunit;

namespace Buttercup.Web.Controllers.Queries;

[Collection(nameof(DatabaseCollection))]
public sealed class HomeControllerQueriesTests(DatabaseFixture<DatabaseCollection> databaseFixture)
    : DatabaseTests<DatabaseCollection>(databaseFixture)
{
    private readonly ModelFactory modelFactory = new();
    private readonly HomeControllerQueries queries = new();

    #region GetRecentlyAddedRecipes

    [Fact]
    public async Task GetRecentlyAddedRecipes_ReturnsNonDeletedRecipesInReverseChronologicalOrder()
    {
        var allRecipes = new List<Recipe>();

        for (var i = 0; i < 15; i++)
        {
            allRecipes.Add(this.modelFactory.BuildRecipe(softDeleted: i % 5 == 0) with
            {
                Created = new DateTime(2010, 1, 2, 3, 4, 5).AddHours(36 * i),
            });
        }

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            dbContext.Recipes.AddRange(allRecipes);
            await dbContext.SaveChangesAsync();
        }

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            Assert.Collection(
                await this.queries.GetRecentlyAddedRecipes(dbContext),
                    r => Assert.Equivalent(allRecipes[14], r),
                    r => Assert.Equivalent(allRecipes[13], r),
                    r => Assert.Equivalent(allRecipes[12], r),
                    r => Assert.Equivalent(allRecipes[11], r),
                    r => Assert.Equivalent(allRecipes[9], r),
                    r => Assert.Equivalent(allRecipes[8], r),
                    r => Assert.Equivalent(allRecipes[7], r),
                    r => Assert.Equivalent(allRecipes[6], r),
                    r => Assert.Equivalent(allRecipes[4], r),
                    r => Assert.Equivalent(allRecipes[3], r));
        }
    }

    #endregion

    #region GetRecentlyUpdatedRecipes

    [Fact]
    public async Task GetRecentlyUpdatedRecipes_ReturnsRecipesInReverseChronologicalOrder()
    {
        var baseDateTime = this.modelFactory.NextDateTime();

        Recipe BuildRecipe(int createdDaysAgo, int modifiedDaysAgo, bool softDeleted = false) =>
            this.modelFactory.BuildRecipe(softDeleted: softDeleted) with
            {
                Created = baseDateTime.AddDays(-createdDaysAgo),
                Modified = baseDateTime.AddDays(-modifiedDaysAgo),
            };

        var allRecipes = new[]
        {
            BuildRecipe(0, 12),
            BuildRecipe(0, 11),
            BuildRecipe(0, 1),
            BuildRecipe(0, 3), // explicitly excluded
            BuildRecipe(1, 13),
            BuildRecipe(1, 2, true), // soft-deleted
            BuildRecipe(7, 7), // never-updated
            BuildRecipe(1, 14),
            BuildRecipe(0, 5), // explicitly excluded
            BuildRecipe(0, 6),
            BuildRecipe(1, 16),
            BuildRecipe(4, 4), // never-updated
            BuildRecipe(1, 8),
            BuildRecipe(2, 10),
            BuildRecipe(2, 9),
            BuildRecipe(1, 15),
        };

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            dbContext.Recipes.AddRange(allRecipes);
            await dbContext.SaveChangesAsync();
        }

        using (var dbContext = this.DatabaseFixture.CreateDbContext())
        {
            var actual = await this.queries.GetRecentlyUpdatedRecipes(
                dbContext, [allRecipes[3].Id, allRecipes[8].Id]);

            Assert.Collection(
                actual,
                r => Assert.Equivalent(allRecipes[2], r),
                r => Assert.Equivalent(allRecipes[9], r),
                r => Assert.Equivalent(allRecipes[12], r),
                r => Assert.Equivalent(allRecipes[14], r),
                r => Assert.Equivalent(allRecipes[13], r),
                r => Assert.Equivalent(allRecipes[1], r),
                r => Assert.Equivalent(allRecipes[0], r),
                r => Assert.Equivalent(allRecipes[4], r),
                r => Assert.Equivalent(allRecipes[7], r),
                r => Assert.Equivalent(allRecipes[15], r));
        }
    }

    #endregion
}
