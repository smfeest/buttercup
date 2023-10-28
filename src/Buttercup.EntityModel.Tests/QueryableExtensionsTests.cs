using Buttercup.TestUtils;
using Xunit;

namespace Buttercup.EntityModel;

public sealed class QueryableExtensionsTests
    : IClassFixture<DatabaseFixture<QueryableExtensionsTests>>
{
    private readonly DatabaseFixture<QueryableExtensionsTests> databaseFixture;
    private readonly ModelFactory modelFactory = new();

    public QueryableExtensionsTests(DatabaseFixture<QueryableExtensionsTests> databaseFixture) =>
        this.databaseFixture = databaseFixture;

    #region FindAsync

    [Fact]
    public async Task FindAsync_EntityFound()
    {
        using var dbContext = this.databaseFixture.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        var expected = this.modelFactory.BuildRecipe();

        dbContext.Recipes.Add(expected);
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();

        var actual = await dbContext.Recipes.AsQueryable().FindAsync(expected.Id);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task FindAsync_EntityNotFound()
    {
        using var dbContext = this.databaseFixture.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        dbContext.Recipes.Add(this.modelFactory.BuildRecipe());
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();

        Assert.Null(await dbContext.Recipes.AsQueryable().FindAsync(this.modelFactory.NextInt()));
    }

    #endregion

    #region GetAsync

    [Fact]
    public async Task GetAsync_EntityFound()
    {
        using var dbContext = this.databaseFixture.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        var expected = this.modelFactory.BuildRecipe();

        dbContext.Recipes.Add(expected);
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();

        var actual = await dbContext.Recipes.GetAsync(expected.Id);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public async Task GetAsync_EntityNotFound()
    {
        using var dbContext = this.databaseFixture.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        dbContext.Recipes.Add(this.modelFactory.BuildRecipe());
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();

        var id = this.modelFactory.NextInt();

        var exception = await Assert.ThrowsAsync<NotFoundException>(
            () => dbContext.Recipes.GetAsync(id));

        Assert.Equal($"Recipe/{id} not found", exception.Message);
    }

    #endregion
}
