using Buttercup.TestUtils;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Buttercup.EntityModel;

public sealed class QueryableExtensionsTests(
    DatabaseFixture<QueryableExtensionsTests> databaseFixture)
    : IClassFixture<DatabaseFixture<QueryableExtensionsTests>>
{
    private readonly DatabaseFixture<QueryableExtensionsTests> databaseFixture = databaseFixture;
    private readonly ModelFactory modelFactory = new();

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

        Assert.Equivalent(expected, actual);
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

        Assert.Equivalent(expected, actual);
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

    #region WhereSoftDeleted / WhereNotSoftDeleted

    [Fact]
    public async Task WhereSoftDeleted_IncludesOnlySoftDeleted_WhereNotSoftDeleted_ExcludesSoftDeleted()
    {
        using var dbContext = this.databaseFixture.CreateDbContext();
        using var transaction = await dbContext.Database.BeginTransactionAsync();

        var visibleRecipe = this.modelFactory.BuildRecipe(softDeleted: false);
        var deletedRecipe = this.modelFactory.BuildRecipe(softDeleted: true);

        dbContext.Recipes.AddRange(visibleRecipe, deletedRecipe);
        await dbContext.SaveChangesAsync();
        dbContext.ChangeTracker.Clear();

        Assert.Equivalent(
            deletedRecipe, await dbContext.Recipes.WhereSoftDeleted().SingleAsync());
        Assert.Equivalent(
            visibleRecipe, await dbContext.Recipes.WhereNotSoftDeleted().SingleAsync());
    }

    #endregion
}
