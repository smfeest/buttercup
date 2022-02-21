using Buttercup.DataAccess;
using Buttercup.Models;

namespace Buttercup.Web.Api;

public class RecipeLoader : BatchDataLoader<long, Recipe>, IRecipeLoader
{
    private readonly IMySqlConnectionSource mySqlConnectionSource;
    private readonly IRecipeDataProvider recipeDataProvider;

    public RecipeLoader(
        IMySqlConnectionSource mySqlConnectionSource,
        IRecipeDataProvider recipeDataProvider,
        IBatchScheduler batchScheduler,
        DataLoaderOptions? options = null)
        : base(batchScheduler, options)
    {
        this.mySqlConnectionSource = mySqlConnectionSource;
        this.recipeDataProvider = recipeDataProvider;
    }

    protected override async Task<IReadOnlyDictionary<long, Recipe>> LoadBatchAsync(
        IReadOnlyList<long> keys, CancellationToken cancellationToken)
    {
        using var connection = await this.mySqlConnectionSource.OpenConnection();

        var recipes = await this.recipeDataProvider.GetRecipes(connection, keys);

        return recipes.ToDictionary(x => x.Id);
    }
}
