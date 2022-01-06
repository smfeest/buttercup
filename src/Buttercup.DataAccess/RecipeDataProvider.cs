using Buttercup.Models;
using MySqlConnector;

namespace Buttercup.DataAccess;

/// <summary>
/// The default implementation of <see cref="IRecipeDataProvider" />.
/// </summary>
internal sealed class RecipeDataProvider : IRecipeDataProvider
{
    private readonly IClock clock;

    public RecipeDataProvider(IClock clock) => this.clock = clock;

    /// <inheritdoc />
    public async Task<long> AddRecipe(MySqlConnection connection, Recipe recipe)
    {
        using var command = connection.CreateCommand();

        command.CommandText = @"INSERT recipe (title, preparation_minutes, cooking_minutes, servings, ingredients, method, suggestions, remarks, source, created, created_by_user_id, modified, modified_by_user_id)
            VALUES (@title, @preparation_minutes, @cooking_minutes, @servings, @ingredients, @method, @suggestions, @remarks, @source, @created, @created_by_user_id, @created, @created_by_user_id)";

        AddInsertUpdateParameters(command, recipe);
        command.Parameters.AddWithValue("@created", recipe.Created);
        command.Parameters.AddWithValue("@created_by_user_id", recipe.CreatedByUserId);

        await command.ExecuteNonQueryAsync();

        return command.LastInsertedId;
    }

    /// <inheritdoc />
    public async Task DeleteRecipe(MySqlConnection connection, long id, int revision)
    {
        using var command = connection.CreateCommand();

        command.CommandText = "DELETE FROM recipe WHERE id = @id AND revision = @revision";
        command.Parameters.AddWithValue("@id", id);
        command.Parameters.AddWithValue("@revision", revision);

        if (await command.ExecuteNonQueryAsync() == 0)
        {
            throw await ConcurrencyOrNotFoundException(connection, id, revision);
        }
    }

    /// <inheritdoc />
    public async Task<Recipe> GetRecipe(MySqlConnection connection, long id)
    {
        using var command = connection.CreateCommand();

        command.CommandText = "SELECT * FROM recipe WHERE id = @id";
        command.Parameters.AddWithValue("@id", id);

        using var reader = await command.ExecuteReaderAsync();

        return await reader.ReadAsync() ?
            ReadRecipe(reader) :
            throw new NotFoundException($"Recipe {id} not found");
    }

    /// <inheritdoc />
    public Task<IList<Recipe>> GetRecipes(MySqlConnection connection) =>
        GetRecipes(connection, "SELECT * FROM recipe ORDER BY title");

    /// <inheritdoc />
    public Task<IList<Recipe>> GetRecentlyAddedRecipes(MySqlConnection connection) =>
        GetRecipes(connection, "SELECT * FROM recipe ORDER BY created DESC LIMIT 10");

    /// <inheritdoc />
    public Task<IList<Recipe>> GetRecentlyUpdatedRecipes(MySqlConnection connection)
    {
        var query = @"SELECT *
            FROM recipe
            LEFT JOIN (SELECT id AS added_id FROM recipe ORDER BY created DESC LIMIT 10) AS added ON added_id = id
            WHERE created != modified AND added_id IS NULL
            ORDER BY modified DESC LIMIT 10";

        return GetRecipes(connection, query);
    }

    /// <inheritdoc />
    public async Task UpdateRecipe(MySqlConnection connection, Recipe recipe)
    {
        using var command = connection.CreateCommand();

        command.CommandText = @"UPDATE recipe
            SET title = @title, preparation_minutes = @preparation_minutes,
                cooking_minutes = @cooking_minutes, servings = @servings,
                ingredients = @ingredients, method = @method, suggestions = @suggestions,
                remarks = @remarks, source = @source, modified = @modified,
                modified_by_user_id = @modified_by_user_id, revision = revision + 1
            WHERE id = @id AND revision = @revision";

        AddInsertUpdateParameters(command, recipe);
        command.Parameters.AddWithValue("@id", recipe.Id);
        command.Parameters.AddWithValue("@modified", recipe.Modified);
        command.Parameters.AddWithValue("@modified_by_user_id", recipe.ModifiedByUserId);
        command.Parameters.AddWithValue("@revision", recipe.Revision);

        if (await command.ExecuteNonQueryAsync() == 0)
        {
            throw await ConcurrencyOrNotFoundException(
                connection, recipe.Id, recipe.Revision);
        }
    }

    private static void AddInsertUpdateParameters(MySqlCommand command, Recipe recipe)
    {
        command.Parameters.AddWithStringValue("@title", recipe.Title);
        command.Parameters.AddWithValue("@preparation_minutes", recipe.PreparationMinutes);
        command.Parameters.AddWithValue("@cooking_minutes", recipe.CookingMinutes);
        command.Parameters.AddWithValue("@servings", recipe.Servings);
        command.Parameters.AddWithStringValue("@ingredients", recipe.Ingredients);
        command.Parameters.AddWithStringValue("@method", recipe.Method);
        command.Parameters.AddWithStringValue("@suggestions", recipe.Suggestions);
        command.Parameters.AddWithStringValue("@remarks", recipe.Remarks);
        command.Parameters.AddWithStringValue("@source", recipe.Source);
    }

    private static async Task<Exception> ConcurrencyOrNotFoundException(
        MySqlConnection connection, long id, int revision)
    {
        using var command = connection.CreateCommand();

        command.CommandText = "SELECT revision FROM recipe WHERE id = @id";
        command.Parameters.AddWithValue("@id", id);

        var currentRevision = await command.ExecuteScalarAsync();

        return currentRevision == null ?
            new NotFoundException($"Recipe {id} not found") :
            new ConcurrencyException(
                $"Revision {revision} does not match current revision {currentRevision}");
    }

    private static async Task<IList<Recipe>> GetRecipes(MySqlConnection connection, string query)
    {
        using var command = connection.CreateCommand();

        command.CommandText = query;

        using var reader = await command.ExecuteReaderAsync();

        var recipes = new List<Recipe>();

        while (await reader.ReadAsync())
        {
            recipes.Add(ReadRecipe(reader));
        }

        return recipes;
    }

    private static Recipe ReadRecipe(MySqlDataReader reader) =>
        new()
        {
            Id = reader.GetInt64("id"),
            Title = reader.GetString("title"),
            PreparationMinutes = reader.GetNullableInt32("preparation_minutes"),
            CookingMinutes = reader.GetNullableInt32("cooking_minutes"),
            Servings = reader.GetNullableInt32("servings"),
            Ingredients = reader.GetString("ingredients"),
            Method = reader.GetString("method"),
            Suggestions = reader.GetNullableString("suggestions"),
            Remarks = reader.GetNullableString("remarks"),
            Source = reader.GetNullableString("source"),
            Created = reader.GetDateTime("created"),
            CreatedByUserId = reader.GetNullableInt64("created_by_user_id"),
            Modified = reader.GetDateTime("modified"),
            ModifiedByUserId = reader.GetNullableInt64("modified_by_user_id"),
            Revision = reader.GetInt32("revision"),
        };
}
